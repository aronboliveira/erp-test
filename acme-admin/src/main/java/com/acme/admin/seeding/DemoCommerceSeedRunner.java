package com.acme.admin.seeding;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.boot.CommandLineRunner;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Component;
import org.springframework.transaction.PlatformTransactionManager;
import org.springframework.transaction.support.TransactionTemplate;

import java.time.*;
import java.util.*;
import java.util.concurrent.ThreadLocalRandom;

@Component
public final class DemoCommerceSeedRunner implements CommandLineRunner {

    private static final int TARGET_PRODUCTS = 24;
    private static final int TARGET_REVENUES = 180;
    private static final int TARGET_EXPENSES = 220;
    private static final int TARGET_BILLING_EVENTS = 120;

    private final JdbcTemplate jdbc;
    private final ObjectMapper om;
    private final TransactionTemplate tx;

    public DemoCommerceSeedRunner(JdbcTemplate jdbc, ObjectMapper om, PlatformTransactionManager tm) {
        this.jdbc = jdbc;
        this.om = om;
        this.tx = new TransactionTemplate(tm);
    }

    @Override
    public void run(String... args) {
        tx.executeWithoutResult((status) -> {
            try {
                if (!tableExists("product_or_services")) return;
                seedProductsIfNeeded();
                seedBudgetsIfNeeded();
                seedFinanceEventsIfNeeded();
                seedBillingEventsIfNeeded();
            } catch (Exception e) {
                System.err.println("DemoCommerceSeedRunner failed: " + e.getMessage());
                status.setRollbackOnly();
            }
        });
    }

    private boolean tableExists(String table) {
        try (var c = Objects.requireNonNull(jdbc.getDataSource()).getConnection()) {
            final var md = c.getMetaData();
            for (final String name : List.of(table, table.toUpperCase(Locale.ROOT), table.toLowerCase(Locale.ROOT))) {
                try (var rs = md.getTables(null, null, name, new String[]{"TABLE"})) {
                    if (rs.next()) return true;
                }
            }
            return false;
        } catch (Exception e) {
            return false;
        }
    }


    private void seedProductsIfNeeded() {
        final long existing = count("select count(*) from product_or_services");
        if (existing >= TARGET_PRODUCTS) return;

        final UUID catProducts = idByCode("product_or_service_categories", "PRODUCTS");
        final UUID catServices = idByCode("product_or_service_categories", "SERVICES");
        final UUID catSubs = idByCode("product_or_service_categories", "SUBSCRIPTIONS");

        final List<Map<String, Object>> items = List.of(
            m("PRODUCT", "SKU-CHAIR-01", "Ergo Chair", 79900L, catProducts),
            m("PRODUCT", "SKU-MON-27", "Monitor 27\"", 139900L, catProducts),
            m("PRODUCT", "SKU-KBD-01", "Mechanical Keyboard", 34900L, catProducts),
            m("PRODUCT", "SKU-MOUSE-01", "Mouse Pro", 15900L, catProducts),
            m("SERVICE", "SKU-CONS-01", "Consulting Hour", 22000L, catServices),
            m("SERVICE", "SKU-IMPL-01", "Implementation Package", 180000L, catServices),
            m("SERVICE", "SKU-MAINT-01", "Maintenance (Monthly)", 89000L, catServices),
            m("SERVICE", "SKU-SUP-01", "Support (Monthly)", 59000L, catServices),
            m("SERVICE", "SKU-AUD-01", "Audit & Review", 240000L, catServices),
            m("SERVICE", "SKU-TRAIN-01", "Training Session", 120000L, catServices),
            m("SERVICE", "SKU-MIG-01", "Migration Service", 320000L, catServices),
            m("SERVICE", "SKU-API-01", "API Integration", 280000L, catServices),
            m("SERVICE", "SKU-DES-01", "Design Sprint", 210000L, catServices),
            m("PRODUCT", "SKU-DOCK-01", "USB-C Dock", 29900L, catProducts),
            m("PRODUCT", "SKU-CAM-01", "Webcam HD", 19900L, catProducts),
            m("PRODUCT", "SKU-MIC-01", "Microphone", 25900L, catProducts),
            m("PRODUCT", "SKU-LGT-01", "Desk Light", 12900L, catProducts),
            m("PRODUCT", "SKU-SSD-01", "External SSD 1TB", 49900L, catProducts),
            m("SERVICE", "SKU-CLOUD-01", "Cloud Setup", 350000L, catServices),
            m("SERVICE", "SKU-SEC-01", "Security Hardening", 390000L, catServices),
            m("SERVICE", "SKU-OPS-01", "Ops Retainer", 160000L, catServices),
            m("SERVICE", "SKU-UX-01", "UX Workshop", 150000L, catServices),
            m("SERVICE", "SKU-ANL-01", "Analytics Setup", 140000L, catServices),
            m("SERVICE", "SKU-SEO-01", "SEO Audit", 90000L, catServices)
        );

        for (var it : items) {
            jdbc.update("""
                insert into product_or_services
                  (id, category_id, kind, sku, title, unit_price_cents, active)
                values
                  (?, ?, ?, ?, ?, ?, true)
                on conflict (sku) do nothing
            """,
                UUID.randomUUID(),
                it.get("categoryId"),
                it.get("kind"),
                it.get("sku"),
                it.get("title"),
                it.get("price")
            );
        }

        for (int i = 1; i <= 4; i++) {
            final String sku = "SKU-SUB-" + String.format("%02d", i);
            jdbc.update("""
                insert into product_or_services
                  (id, category_id, kind, sku, title, unit_price_cents, active)
                values
                  (?, ?, 'SERVICE', ?, ?, ?, true)
                on conflict (sku) do nothing
            """, UUID.randomUUID(), catSubs, sku, "Subscription Tier " + i, 4900L * i);
        }
    }

    private void seedBudgetsIfNeeded() {
        final long existing = count("select count(*) from budgets");
        if (existing >= 4) return;

        final LocalDate now = LocalDate.now();
        final LocalDate start = now.withDayOfMonth(1);

        insertBudget("Q1 Budget", now.withMonth(1).withDayOfMonth(1), now.withMonth(3).withDayOfMonth(31), 3_500_000L, 1_900_000L);
        insertBudget("Q2 Budget", now.withMonth(4).withDayOfMonth(1), now.withMonth(6).withDayOfMonth(30), 4_200_000L, 2_100_000L);
        insertBudget("Q3 Budget", now.withMonth(7).withDayOfMonth(1), now.withMonth(9).withDayOfMonth(30), 4_800_000L, 2_450_000L);
        insertBudget("Current Month", start, start.plusMonths(1).minusDays(1), 1_600_000L, 900_000L);
    }

    private void seedFinanceEventsIfNeeded() {
        final long rev = count("select count(*) from revenues");
        final long exp = count("select count(*) from expenses");

        final UUID ecOrder = idByCode("expense_categories", "ORDER");
        final UUID ecPurchase = idByCode("expense_categories", "PURCHASE");
        final UUID ecHiring = idByCode("expense_categories", "HIRING");
        final UUID ecBill = idByCode("expense_categories", "BILL");
        final UUID ecTax = idByCode("expense_categories", "TAX");
        final UUID ecOps = idByCode("expense_categories", "OPS");
        final UUID ecMarketing = idByCode("expense_categories", "MARKETING");

        final List<UUID> taxIds = listIds("select id from taxes order by code");

        final Instant now = Instant.now();

        if (rev < TARGET_REVENUES) {
            final int missing = (int) (TARGET_REVENUES - rev);
            for (int i = 0; i < missing; i++) {
                final Instant at = now.minus(Duration.ofHours(6L * i));
                final long cents = gaussianCents(65000L, 42000L, 1200L, 380000L);
                jdbc.update("""
                    insert into revenues (id, occurred_at, source, amount_cents, currency, note)
                    values (?, ?, 'order', ?, 'brl', ?)
                """, UUID.randomUUID(), at, cents, (i % 17 == 0 ? "promo spike" : null));
            }
            jdbc.update("""
                insert into revenues (id, occurred_at, source, amount_cents, currency, note)
                values (?, ?, 'invoice', ?, 'brl', 'enterprise deal')
            """, UUID.randomUUID(), now.minus(Duration.ofDays(8)), 1_450_000L);
        }

        if (exp < TARGET_EXPENSES) {
            final int missing = (int) (TARGET_EXPENSES - exp);
            for (int i = 0; i < missing; i++) {
                final Instant at = now.minus(Duration.ofHours(5L * i));
                final long cents = gaussianCents(42000L, 36000L, 800L, 320000L);
                final UUID cat = pickOne(List.of(ecPurchase, ecBill, ecOps, ecMarketing, ecTax, ecHiring));
                jdbc.update("""
                    insert into expenses (id, category_id, occurred_at, amount_cents, currency, note)
                    values (?, ?, ?, ?, 'brl', ?)
                """, UUID.randomUUID(), cat, at, cents, (i % 19 == 0 ? "monthly renewal" : null));
            }
        }

        seedTaxableEntityAndExpense("orders", "ORD", ecOrder, taxIds, 28);
        seedTaxableEntityAndExpense("purchases", "PUR", ecPurchase, taxIds, 24);
        seedTaxableEntityAndExpense("hirings", "HIR", ecHiring, taxIds, 18);
        seedTaxableEntityAndExpense("bills", "BILL", ecBill, taxIds, 22);

        final long inv = count("select count(*) from invoices");
        if (inv < 40) {
            for (int i = 0; i < 40; i++) {
                final Instant at = Instant.now().minus(Duration.ofDays(i));
                final String code = "INV-" + String.format("%04d", 1000 + i);
                jdbc.update("""
                    insert into invoices (id, code, occurred_at, gross_cents, currency)
                    values (?, ?, ?, ?, 'brl')
                    on conflict (code) do nothing
                """, UUID.randomUUID(), code, at, gaussianCents(110000L, 65000L, 8000L, 650000L));
            }
        }
    }

    private void seedBillingEventsIfNeeded() {
        final long existing = count("select count(*) from billing_events");
        if (existing >= TARGET_BILLING_EVENTS) return;

        final int missing = (int) (TARGET_BILLING_EVENTS - existing);
        final Instant now = Instant.now();

        final List<String> types = List.of(
            "payment_intent.created",
            "payment_intent.succeeded",
            "payment_intent.payment_failed",
            "charge.succeeded",
            "charge.refunded",
            "customer.created",
            "customer.subscription.created",
            "invoice.paid"
        );

        for (int i = 0; i < missing; i++) {
            final String t = types.get(i % types.size());
            final String eid = "evt_demo_" + UUID.randomUUID();
            final Instant at = now.minus(Duration.ofHours(3L * i));

            final Map<String, Object> payload = Map.of(
                "id", eid,
                "type", t,
                "livemode", false,
                "created", at.getEpochSecond()
            );

            jdbc.update("""
                insert into billing_events (id, provider, event_id, event_type, payload, received_at)
                values (?, 'stripe', ?, ?, ?::jsonb, ?)
                on conflict (provider, event_id) do nothing
            """,
                UUID.randomUUID(),
                eid,
                t,
                safeJson(payload),
                at
            );
        }
    }

    private void seedTaxableEntityAndExpense(
        String table,
        String prefix,
        UUID expenseCategoryId,
        List<UUID> allTaxIds,
        int n
    ) {
        final long existing = count("select count(*) from " + table);
        if (existing >= n) return;

        for (int i = 0; i < n; i++) {
            final UUID id = UUID.randomUUID();
            final String code = prefix + "-" + String.format("%05d", 100 + i);
            final Instant at = Instant.now().minus(Duration.ofDays(i));

            final List<UUID> taxes = pickMany(allTaxIds, 0, 3);
            final String taxJson = taxes.isEmpty() ? null : safeJson(taxes.stream().map(UUID::toString).toList());

            final long gross = gaussianCents(160000L, 95000L, 12000L, 890000L);

            jdbc.update("""
                insert into %s (id, code, occurred_at, gross_cents, currency, tax_ids)
                values (?, ?, ?, ?, 'brl', ?::jsonb)
                on conflict (code) do nothing
            """.formatted(table),
                id, code, at, gross, taxJson
            );

            jdbc.update("""
                insert into expenses (id, category_id, occurred_at, amount_cents, currency, note, source_type, source_id)
                values (?, ?, ?, ?, 'brl', ?, ?, ?)
            """,
                UUID.randomUUID(),
                expenseCategoryId,
                at,
                Math.max(1000L, gross / 10),
                "auto from " + prefix,
                table.toUpperCase(),
                id
            );
        }
    }

    private void insertBudget(String title, LocalDate start, LocalDate end, long targetRev, long maxExp) {
        jdbc.update("""
            insert into budgets (id, title, period_start, period_end, currency, target_revenue_cents, max_expense_cents)
            values (?, ?, ?, ?, 'brl', ?, ?)
            on conflict do nothing
        """, UUID.randomUUID(), title, start, end, targetRev, maxExp);
    }

    private UUID idByCode(String table, String code) {
        return jdbc.queryForObject(
            "select id from " + table + " where code = ? limit 1",
            (rs, i) -> UUID.fromString(rs.getString("id")),
            code
        );
    }

    private long count(String sql) {
        final Long v = jdbc.queryForObject(sql, Long.class);
        return v == null ? 0L : v;
    }

    private List<UUID> listIds(String sql) {
        return jdbc.query(sql, (rs, i) -> UUID.fromString(rs.getString(1)));
    }

    private static Map<String, Object> m(String kind, String sku, String title, long price, UUID categoryId) {
        final Map<String, Object> out = new HashMap<>();
        out.put("kind", kind);
        out.put("sku", sku);
        out.put("title", title);
        out.put("price", price);
        out.put("categoryId", categoryId);
        return out;
    }

    private String safeJson(Object v) {
        try {
            return om.writeValueAsString(v);
        } catch (Exception e) {
            return "{}";
        }
    }

    private static UUID pickOne(List<UUID> ids) {
        return ids.get(ThreadLocalRandom.current().nextInt(ids.size()));
    }

    private static List<UUID> pickMany(List<UUID> ids, int min, int max) {
        final int size = ThreadLocalRandom.current().nextInt(min, max + 1);
        if (size <= 0) return List.of();
        final List<UUID> copy = new ArrayList<>(ids);
        Collections.shuffle(copy, new Random(1337L + size));
        return copy.subList(0, Math.min(size, copy.size()));
    }

    private static long gaussianCents(long mean, long std, long min, long max) {
        final double g = ThreadLocalRandom.current().nextGaussian();
        long v = Math.round(mean + (g * std));
        if (v < min) v = min;
        if (v > max) v = max;
        return v;
    }
}
