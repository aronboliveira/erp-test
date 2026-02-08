package com.acme.admin.security.stripe;

import com.acme.admin.dto.StripePaymentDtos;
import org.springframework.stereotype.Component;

import java.util.UUID;

@Component
public final class StripeNoopGateway implements StripeGateway {

    @Override
    public StripePaymentDtos.PaymentIntentResult createPaymentIntent(StripePaymentDtos.CreatePaymentIntentRequest req) {
        final String id = "pi_noop_" + UUID.randomUUID();
        final String secret = "pi_noop_secret_" + UUID.randomUUID();
        return new StripePaymentDtos.PaymentIntentResult("stripe", id, secret, "requires_payment_method");
    }
}
