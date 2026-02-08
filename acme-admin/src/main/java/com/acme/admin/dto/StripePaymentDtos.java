package com.acme.admin.dto;

public final class StripePaymentDtos {
    private StripePaymentDtos() {}

    public record CreatePaymentIntentRequest(
        String currency,
        long amountCents,
        String customerEmail,
        String description
    ) {}

    public record CreatePaymentIntentResponse(
        String provider,
        String publishableKey,
        String paymentIntentId,
        String clientSecret,
        String status
    ) {}

    public record PaymentIntentResult(
        String provider,
        String paymentIntentId,
        String clientSecret,
        String status
    ) {}
}
