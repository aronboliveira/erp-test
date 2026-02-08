package com.acme.admin.security.stripe;

import com.acme.admin.dto.StripePaymentDtos;

public interface StripeGateway {
    StripePaymentDtos.PaymentIntentResult createPaymentIntent(StripePaymentDtos.CreatePaymentIntentRequest req);
}
