package com.acme.admin.controller;

import com.acme.admin.dto.BillingDtos;
import com.acme.admin.service.BillingService;
import org.springframework.http.*;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/billing")
public class BillingController {

    private final BillingService svc;

    public BillingController(BillingService svc) {
        this.svc = svc;
    }

    @PostMapping("/checkout-session")
    @PreAuthorize("hasAuthority('BILLING_CREATE')")
    public ResponseEntity<BillingDtos.CheckoutSessionResponse> createCheckoutSession(
        @RequestBody BillingDtos.CreateCheckoutSessionRequest req
    ) throws Exception {
        return ResponseEntity.status(HttpStatus.CREATED).body(svc.createCheckoutSession(req));
    }
}
