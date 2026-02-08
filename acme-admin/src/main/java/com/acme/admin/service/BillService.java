package com.acme.admin.service;

import com.acme.admin.domain.BillEntity;
import com.acme.admin.dto.BillCreateRequest;
import com.acme.admin.repository.BillRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class BillService {
    private final BillRepository repo;

    public BillService(BillRepository repo) {
        this.repo = repo;
    }

    public List<BillEntity> list() {
        return repo.findAll();
    }

    public BillEntity create(BillCreateRequest req) {
        final BillEntity e = new BillEntity();
        e.setCode(req.code());
        e.setOccurredAt(req.occurredAt());
        e.setCurrency(req.currency());
        e.setTotal(req.total());
        e.setTaxIds(req.taxIds());
        return repo.save(e);
    }
}
