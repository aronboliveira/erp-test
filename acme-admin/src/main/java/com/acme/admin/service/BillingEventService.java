package com.acme.admin.service;

import com.acme.admin.domain.BillingEventEntity;
import com.acme.admin.dto.BillingEventDtos;
import com.acme.admin.repository.BillingEventRepository;
import com.acme.admin.time.TemporalFormat;
import org.springframework.data.domain.*;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;

@Service
public class BillingEventService {

    private final BillingEventRepository repo;

    public BillingEventService(BillingEventRepository repo) {
        this.repo = repo;
    }

    @Transactional(readOnly = true)
    public BillingEventDtos.PageResponse page(int page, int size, String provider, String eventType, Instant from, Instant to) {
        final int p = Math.max(0, page);
        final int s = Math.min(Math.max(size, 1), 50);

        final Page<BillingEventEntity> res =
            repo.findAll(PageRequest.of(p, s, Sort.by(Sort.Direction.DESC, "receivedAt")));

        final var rows = res.getContent().stream().map(e ->
            new BillingEventDtos.BillingEventRow(
                e.getId(),
                e.getProvider(),
                e.getEventId(),
                e.getEventType(),
                TemporalFormat.datetimeLocal(e.getReceivedAt())
            )
        ).toList();

        return new BillingEventDtos.PageResponse(rows, p, s, res.getTotalElements());
    }
}
