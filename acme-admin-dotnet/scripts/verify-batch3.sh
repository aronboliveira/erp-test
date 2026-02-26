#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

BACKEND_PORT="${BATCH3_BACKEND_PORT:-18080}"
FRONTEND_PORT="${BATCH3_FRONTEND_PORT:-14000}"
POSTGRES_PORT="${BATCH3_POSTGRES_PORT:-5434}"

log() {
  printf '%s\n' "$*"
}

cleanup() {
  BACKEND_PORT="$BACKEND_PORT" FRONTEND_PORT="$FRONTEND_PORT" POSTGRES_PORT="$POSTGRES_PORT" docker compose down >/dev/null 2>&1 || true
}
trap cleanup EXIT

assert_status() {
  local name="$1"
  local expected="$2"
  local code="$3"

  if [[ "$code" != "$expected" ]]; then
    log "FAIL: $name expected HTTP $expected, got $code"
    exit 1
  fi

  log "PASS: $name -> HTTP $code"
}

log "[1/5] Building app image"
docker compose build app >/tmp/batch3-build.log

log "[2/5] Starting stack in strict auth mode"
BACKEND_PORT="$BACKEND_PORT" \
FRONTEND_PORT="$FRONTEND_PORT" \
POSTGRES_PORT="$POSTGRES_PORT" \
AUTH_ENABLE_MOCK_HEADER=true \
Stripe__PublishableKey="pk_test_noop" \
Billing__Stripe__WebhookSecret="whsec_dev" \
docker compose up -d postgres app >/tmp/batch3-up.log

log "[3/5] Waiting for health endpoint"
for _ in $(seq 1 60); do
  code="$(curl -sS -o /tmp/batch3-health.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/actuator/health" || true)"
  if [[ "$code" == "200" ]]; then
    break
  fi
  sleep 2
done

code="$(curl -sS -o /tmp/batch3-health.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/actuator/health" || true)"
if [[ "$code" != "200" ]]; then
  log "FAIL: health endpoint did not become ready"
  docker compose logs --no-color --tail=120 app || true
  exit 1
fi
assert_status "health" "200" "$code"

log "[4/5] Running API checks"

code="$(curl -sS -o /tmp/batch3-r1.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/finance/revenue" || true)"
assert_status "strict auth rejects anonymous request" "401" "$code"

test_user="batch3_user_$(date +%s)"
test_pass="Batch3Pass!123"
code="$(curl -sS -H 'Content-Type: application/json' -H 'X-Mock-User: admin' -H 'X-Mock-Perms: users.write' \
  -d "{\"email\":\"${test_user}@acme.local\",\"username\":\"${test_user}\",\"displayName\":\"Batch3 User\",\"password\":\"${test_pass}\",\"roleNames\":[\"SuperAdmin\"]}" \
  -o /tmp/batch3-r2.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/admin/users" || true)"
assert_status "provision test user via mock permission" "200" "$code"

code="$(curl -sS -u "${test_user}:${test_pass}" -o /tmp/batch3-r3.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/me" || true)"
assert_status "basic auth profile" "200" "$code"

code="$(curl -sS -u "${test_user}:${test_pass}" -o /tmp/batch3-r3b.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/admin/roles" || true)"
assert_status "basic auth role admin endpoint" "200" "$code"

code="$(curl -sS -H 'X-Mock-User: admin' -H 'X-Mock-Perms: finance.read' \
  -o /tmp/batch3-r3c.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/taxes" || true)"
assert_status "tax list denied without taxes.read permission" "403" "$code"

code="$(curl -sS -H 'X-Mock-User: admin' -H 'X-Mock-Perms: taxes.read' \
  -o /tmp/batch3-r3d.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/taxes" || true)"
assert_status "tax list allowed with taxes.read permission" "200" "$code"

code="$(curl -sS -H 'X-Mock-User: admin' -H 'X-Mock-Perms: orders.read' \
  -o /tmp/batch3-r3e.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/sales/orders" || true)"
assert_status "orders list allowed with orders.read permission" "200" "$code"

code="$(curl -sS -H 'X-Mock-User: admin' -H 'X-Mock-Perms: procurement.read' \
  -o /tmp/batch3-r3f.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/procurement/purchases" || true)"
assert_status "purchases list allowed with procurement.read permission" "200" "$code"

code="$(curl -sS -H 'X-Mock-User: admin' -H 'X-Mock-Perms: hr.read' \
  -o /tmp/batch3-r3g.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/hr/hirings" || true)"
assert_status "hirings list allowed with hr.read permission" "200" "$code"

manager_user="batch3_mgr_$(date +%s)"
manager_pass="Batch3Mgr!123"
code="$(curl -sS -H 'Content-Type: application/json' -H 'X-Mock-User: admin' -H 'X-Mock-Perms: users.write' \
  -d "{\"email\":\"${manager_user}@acme.local\",\"username\":\"${manager_user}\",\"displayName\":\"Batch3 Manager\",\"password\":\"${manager_pass}\",\"roleNames\":[\"MANAGER\"]}" \
  -o /tmp/batch3-r3h.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/admin/users" || true)"
assert_status "provision manager role user" "200" "$code"

code="$(curl -sS -u "${manager_user}:${manager_pass}" -o /tmp/batch3-r3i.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/finance/revenue" || true)"
assert_status "manager can read finance revenue" "200" "$code"

code="$(curl -sS -u "${manager_user}:${manager_pass}" -o /tmp/batch3-r3j.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/admin/users" || true)"
assert_status "manager denied admin user list" "403" "$code"

viewer_user="batch3_view_$(date +%s)"
viewer_pass="Batch3View!123"
code="$(curl -sS -H 'Content-Type: application/json' -H 'X-Mock-User: admin' -H 'X-Mock-Perms: users.write' \
  -d "{\"email\":\"${viewer_user}@acme.local\",\"username\":\"${viewer_user}\",\"displayName\":\"Batch3 Viewer\",\"password\":\"${viewer_pass}\",\"roleNames\":[\"VIEWER\"]}" \
  -o /tmp/batch3-r3k.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/admin/users" || true)"
assert_status "provision viewer role user" "200" "$code"

code="$(curl -sS -u "${viewer_user}:${viewer_pass}" -o /tmp/batch3-r3l.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/finance/revenue" || true)"
assert_status "viewer can read finance revenue" "200" "$code"

code="$(curl -sS -u "${viewer_user}:${viewer_pass}" -H 'Content-Type: application/json' \
  -d '{"occurredAt":"2025-01-01T00:00:00Z","amount":12.50,"currency":"USD","sourceRef":"viewer"}' \
  -o /tmp/batch3-r3m.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/finance/revenue" || true)"
assert_status "viewer denied finance write" "403" "$code"

code="$(curl -sS -H 'Content-Type: application/json' -H 'X-Mock-User: admin' -H 'X-Mock-Perms: billing.create' \
  -d "{\"currency\":\"usd\",\"items\":[{\"name\":\"Plan\",\"unitAmountCents\":9900,\"quantity\":1}],\"successUrl\":\"http://localhost:${FRONTEND_PORT}/s\",\"cancelUrl\":\"http://localhost:${FRONTEND_PORT}/c\"}" \
  -o /tmp/batch3-r4.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/billing/checkout-session" || true)"
assert_status "checkout session" "201" "$code"

code="$(curl -sS -H 'Content-Type: application/json' -H 'X-Mock-User: admin' -H 'X-Mock-Perms: billing.pay' \
  -d '{"currency":"usd","amountCents":1200,"customerEmail":"test@example.com","description":"test"}' \
  -o /tmp/batch3-r5.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/billing/payment-intents" || true)"
assert_status "payment intent" "200" "$code"

payload='{"id":"evt_batch3_ok","type":"checkout.session.completed"}'
ts="$(date +%s)"
sig="$(printf '%s.%s' "$ts" "$payload" | openssl dgst -sha256 -hmac 'whsec_dev' -binary | xxd -p -c 256)"
code="$(curl -sS -H 'Content-Type: application/json' -H "Stripe-Signature: t=${ts},v1=${sig}" \
  -d "$payload" -o /tmp/batch3-r6.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/billing/webhook" || true)"
assert_status "webhook valid signature" "200" "$code"

code="$(curl -sS -H 'Content-Type: application/json' -H "Stripe-Signature: t=${ts},v1=deadbeef" \
  -d '{"id":"evt_batch3_bad","type":"checkout.session.completed"}' \
  -o /tmp/batch3-r7.out -w '%{http_code}' "http://localhost:${BACKEND_PORT}/api/billing/webhook" || true)"
assert_status "webhook invalid signature" "400" "$code"

log "[5/5] Batch 3 verification complete"
