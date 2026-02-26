-- =============================================================================
-- V003: Billing Events - Stripe integration and payment tracking
-- =============================================================================

CREATE TABLE billing_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    provider VARCHAR(30) NOT NULL DEFAULT 'STRIPE',
    event_type VARCHAR(60) NOT NULL,
    external_id VARCHAR(255),
    payload JSONB,
    received_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_billing_events_provider ON billing_events(provider);
CREATE INDEX idx_billing_events_type ON billing_events(event_type);
CREATE INDEX idx_billing_events_external ON billing_events(external_id);
CREATE INDEX idx_billing_events_received ON billing_events(received_at);
