package com.acme.admin.domain;

import org.springframework.data.jpa.domain.Specification;

import java.time.Instant;

public final class BillingEventSpecs {
    private BillingEventSpecs() {}

    public static Specification<BillingEventEntity> providerEq(String provider) {
        return (root, q, cb) ->
            provider == null || provider.isBlank()
                ? cb.conjunction()
                : cb.equal(root.get("provider"), provider.trim());
    }

    public static Specification<BillingEventEntity> eventTypeEq(String eventType) {
        return (root, q, cb) ->
            eventType == null || eventType.isBlank()
                ? cb.conjunction()
                : cb.equal(root.get("eventType"), eventType.trim());
    }

    public static Specification<BillingEventEntity> receivedBetween(Instant from, Instant to) {
        return (root, q, cb) -> {
            if (from == null && to == null) return cb.conjunction();
            if (from != null && to == null) return cb.greaterThanOrEqualTo(root.get("receivedAt"), from);
            if (from == null) return cb.lessThanOrEqualTo(root.get("receivedAt"), to);
            return cb.between(root.get("receivedAt"), from, to);
        };
    }
}
