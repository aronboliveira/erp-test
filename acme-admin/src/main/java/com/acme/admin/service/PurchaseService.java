package com.acme.admin.service;

import com.acme.admin.domain.PurchaseEntity;
import com.acme.admin.dto.PurchaseCreateRequest;
import com.acme.admin.repository.PurchaseRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class PurchaseService {
    private final PurchaseRepository repo;

    public PurchaseService(PurchaseRepository repo) {
        this.repo = repo;
    }

    public List<PurchaseEntity> list() {
        return repo.findAll();
    }

    public PurchaseEntity create(PurchaseCreateRequest req) {
        final PurchaseEntity e = new PurchaseEntity();
        e.setCode(req.code());
        e.setOccurredAt(req.occurredAt());
        e.setCurrency(req.currency());
        e.setTotal(req.total());
        e.setVendor(req.vendor());
        e.setTaxIds(req.taxIds());
        return repo.save(e);
    }
}
