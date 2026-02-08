package com.acme.admin.controller;

import com.acme.admin.dto.BillingEventDtos;
import com.acme.admin.service.BillingEventService;
import com.acme.admin.time.DateMapper;
import com.acme.admin.time.DateValidator;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.security.access.prepost.PreAuthorize;

import java.time.Instant;
import java.time.LocalDateTime;

@RestController
@RequestMapping("/api/billing/events")
public class BillingEventController {

    private final BillingEventService svc;

    public BillingEventController(BillingEventService svc) {
        this.svc = svc;
    }

    @GetMapping
    @PreAuthorize("hasAuthority('BILLING_READ')")
    public ResponseEntity<BillingEventDtos.PageResponse> page(
        @RequestParam(defaultValue = "0") int page,
        @RequestParam(defaultValue = "10") int size,
        @RequestParam(required = false) String provider,
        @RequestParam(required = false) String eventType,
        @RequestParam(required = false) String receivedFrom,
        @RequestParam(required = false) String receivedTo
    ) {
        if (page < 0) throw new IllegalArgumentException("page must be >= 0");
        if (size < 1 || size > 100) throw new IllegalArgumentException("size must be between 1 and 100");
        final LocalDateTime fromLdt = receivedFrom != null && DateValidator.isDatetimeLocal(receivedFrom)
            ? DateValidator.parseDatetimeLocal(receivedFrom)
            : null;

        final LocalDateTime toLdt = receivedTo != null && DateValidator.isDatetimeLocal(receivedTo)
            ? DateValidator.parseDatetimeLocal(receivedTo)
            : null;

        DateValidator.assertRange(fromLdt, toLdt);

        final Instant from = DateMapper.toUtcInstant(fromLdt);
        final Instant to = DateMapper.toUtcInstant(toLdt);

        return ResponseEntity.ok(svc.page(page, size, provider, eventType, from, to));
    }
}
