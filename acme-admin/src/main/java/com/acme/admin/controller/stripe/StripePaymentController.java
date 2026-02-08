package com.acme.admin.controller.stripe;

import com.acme.admin.dto.StripePaymentDtos;
import com.acme.admin.service.stripe.StripePaymentService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.security.access.prepost.PreAuthorize;

@RestController
@RequestMapping("/api/billing")
public class StripePaymentController {

    private final StripePaymentService svc;

    public StripePaymentController(StripePaymentService svc) {
        this.svc = svc;
    }

    @PostMapping("/payment-intents")
    @PreAuthorize("hasAuthority('BILLING_PAY')")
    public ResponseEntity<StripePaymentDtos.CreatePaymentIntentResponse> createPaymentIntent(
        @RequestBody StripePaymentDtos.CreatePaymentIntentRequest req
    ) {
        validate(req);
        return ResponseEntity.ok(svc.create(req));
    }

    private static void validate(StripePaymentDtos.CreatePaymentIntentRequest req) {
        if (req == null) throw new IllegalArgumentException("Request required");
        if (req.currency() == null || req.currency().isBlank()) throw new IllegalArgumentException("currency required");
        if (req.amountCents() <= 0) throw new IllegalArgumentException("amountCents must be > 0");
        if (req.amountCents() > 2_000_000_00L) throw new IllegalArgumentException("amountCents too large");
    }
}
