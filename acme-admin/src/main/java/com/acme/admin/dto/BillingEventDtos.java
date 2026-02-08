package com.acme.admin.dto;

import java.util.List;
import java.util.UUID;

public final class BillingEventDtos {
    private BillingEventDtos() {}

    public record BillingEventRow(
        UUID id,
        String provider,
        String eventId,
        String eventType,
        String receivedAt
    ) {}

    public record PageResponse(
        List<BillingEventRow> items,
        int page,
        int size,
        long total
    ) {}
}
