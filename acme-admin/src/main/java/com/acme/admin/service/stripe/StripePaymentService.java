package com.acme.admin.service.stripe;

import com.acme.admin.dto.StripePaymentDtos;
import com.acme.admin.security.stripe.StripeGateway;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

@Service
public class StripePaymentService {

    private final StripeGateway gateway;
    
    @Value("${stripe.publishable-key:}")
    private String publishableKey;

    public StripePaymentService(StripeGateway gateway) {
        this.gateway = gateway;
    }

    public StripePaymentDtos.CreatePaymentIntentResponse create(StripePaymentDtos.CreatePaymentIntentRequest req) {
        if (publishableKey == null || publishableKey.isBlank()) {
            throw new IllegalStateException("stripe.publishable-key not configured");
        }
        
        final var r = gateway.createPaymentIntent(req);
        return new StripePaymentDtos.CreatePaymentIntentResponse(
            r.provider(),
            publishableKey,
            r.paymentIntentId(),
            r.clientSecret(),
            r.status()
        );
    }
}
