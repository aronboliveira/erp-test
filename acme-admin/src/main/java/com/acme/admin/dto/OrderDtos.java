package com.acme.admin.dto;

import java.util.List;
import java.util.UUID;

public final class OrderDtos {
    private OrderDtos() {}

    public record CreateOrderRequest(
        String code,
        String issuedAt,
        List<UUID> taxIds
    ) {}

    public record OrderResponse(
        UUID id,
        String code,
        String issuedAt,
        List<UUID> taxIds
    ) {}
}
