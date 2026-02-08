create table if not exists billing_events (
  id uuid primary key,
  provider text not null,
  event_id text not null,
  event_type text not null,
  payload jsonb null,
  received_at timestamptz not null default now()
);

create index if not exists idx_billing_events_received_at on billing_events(received_at desc);
create index if not exists idx_billing_events_provider on billing_events(provider);
create index if not exists idx_billing_events_type on billing_events(event_type);
create unique index if not exists uq_billing_events_provider_event on billing_events(provider, event_id);
