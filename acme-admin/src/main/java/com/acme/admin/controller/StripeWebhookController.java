package com.acme.admin.controller;

import com.acme.admin.domain.BillingEventEntity;
import com.acme.admin.domain.BillingProperties;
import com.acme.admin.repository.BillingEventRepository;
import com.stripe.model.Event;
import com.stripe.net.Webhook;

import org.springframework.http.ResponseEntity;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

@RestController
@RequestMapping("/api/billing")
public class StripeWebhookController {

    private static final String PROVIDER = "stripe";

    private final BillingProperties props;
    private final BillingEventRepository events;

    public StripeWebhookController(BillingProperties props, BillingEventRepository events) {
        this.props = props;
        this.events = events;
    }

    @PostMapping("/webhook")
    @Transactional
    public ResponseEntity<String> webhook(
        @RequestHeader(name = "Stripe-Signature", required = false) String sig,
        @RequestBody String payload
    ) throws Exception {
        if (sig == null || sig.isBlank()) throw new IllegalArgumentException("stripe: signature required");
        if (payload == null || payload.isBlank()) throw new IllegalArgumentException("stripe: payload required");
        if (props.webhookSecret() == null || props.webhookSecret().isBlank())
            throw new IllegalStateException("stripe: webhook secret not configured");

        final Event ev = Webhook.constructEvent(payload, sig, props.webhookSecret());
        if (events.existsByEventId(ev.getId())) return ResponseEntity.ok("ok");

        events.save(new BillingEventEntity(
            UUID.randomUUID(),
            PROVIDER,
            ev.getId(),
            ev.getType(),
            payload
        ));

        return ResponseEntity.ok("ok");
    }
}
