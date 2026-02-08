package com.acme.admin.domain;

import jakarta.persistence.*;

import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "billing_events")
public class BillingEventEntity {

    @Id
    @Column(nullable = false)
    private UUID id;

    @Column(nullable = false, length = 32)
    private String provider;

    @Column(name = "event_id", nullable = false, unique = true, length = 128)
    private String eventId;

    @Column(name = "event_type", nullable = false, length = 128)
    private String eventType;

    @Column(name = "received_at", nullable = false)
    private Instant receivedAt = Instant.now();

    @Column(nullable = false, columnDefinition = "text")
    private String payload;

    protected BillingEventEntity() {}

    public BillingEventEntity(UUID id, String provider, String eventId, String eventType, String payload) {
        this.id = id;
        this.provider = provider;
        this.eventId = eventId;
        this.eventType = eventType;
        this.payload = payload;
    }

    public UUID getId() { return id; }
    public String getProvider() { return provider; }
    public String getEventId() { return eventId; }
    public String getEventType() { return eventType; }
    public Instant getReceivedAt() { return receivedAt; }
}
