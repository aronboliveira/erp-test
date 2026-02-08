package com.acme.admin.provider;

public interface PaymentProvider {
    CheckoutSessionResult createCheckoutSession(CreateCheckoutSessionCommand cmd) throws Exception;

    record CreateCheckoutSessionCommand(
        String currency,
        String customerEmail,
        LineItem[] items,
        String successUrl,
        String cancelUrl
    ) {}

    record LineItem(
        String name,
        long unitAmountCents,
        long quantity
    ) {}

    record CheckoutSessionResult(
        String provider,
        String sessionId,
        String url
    ) {}
}
