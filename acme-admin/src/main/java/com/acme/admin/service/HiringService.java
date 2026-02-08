package com.acme.admin.service;

import com.acme.admin.domain.HiringEntity;
import com.acme.admin.dto.HiringCreateRequest;
import com.acme.admin.repository.HiringRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class HiringService {
    private final HiringRepository repo;

    public HiringService(HiringRepository repo) {
        this.repo = repo;
    }

    public List<HiringEntity> list() {
        return repo.findAll();
    }

    public HiringEntity create(HiringCreateRequest req) {
        final HiringEntity e = new HiringEntity();
        e.setCode(req.code());
        e.setOccurredAt(req.occurredAt());
        e.setCurrency(req.currency());
        e.setTotal(req.total());
        e.setCandidateName(req.candidateName());
        e.setTaxIds(req.taxIds());
        return repo.save(e);
    }
}
