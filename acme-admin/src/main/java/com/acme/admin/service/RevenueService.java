package com.acme.admin.service;

import com.acme.admin.domain.RevenueEntity;
import com.acme.admin.dto.RevenueCreateRequest;
import com.acme.admin.repository.RevenueRepository;
import java.util.List;
import org.springframework.stereotype.Service;

@Service
public class RevenueService {
  private final RevenueRepository repo;

  public RevenueService(RevenueRepository repo) {
    this.repo = repo;
  }

  public List<RevenueEntity> list() {
    return repo.findAll();
  }

  public RevenueEntity create(RevenueCreateRequest req) {
    final RevenueEntity e = new RevenueEntity();
    e.setOccurredAt(req.occurredAt());
    e.setAmount(req.amount());
    e.setCurrency(req.currency());
    e.setSourceRef(req.sourceRef());
    return repo.save(e);
  }
}
