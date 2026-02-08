package com.acme.admin.dto;

import java.util.List;

public final class BillingDtos {
    private BillingDtos() {}

    public record CreateCheckoutSessionRequest(
        String currency,
        String customerEmail,
        List<LineItem> items,
        String successUrl,
        String cancelUrl
    ) {}

    public record LineItem(
        String name,
        long unitAmountCents,
        long quantity
    ) {}

    public record CheckoutSessionResponse(
        String provider,
        String sessionId,
        String url
    ) {}
}
