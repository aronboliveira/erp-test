package com.acme.admin.service;

import com.acme.admin.domain.BudgetEntity;
import com.acme.admin.dto.BudgetCreateRequest;
import com.acme.admin.repository.BudgetRepository;
import java.util.List;
import org.springframework.stereotype.Service;

@Service
public class BudgetService {
  private final BudgetRepository repo;

  public BudgetService(BudgetRepository repo) {
    this.repo = repo;
  }

  public List<BudgetEntity> list() {
    return repo.findAll();
  }

  public BudgetEntity create(BudgetCreateRequest req) {
    final BudgetEntity e = new BudgetEntity();
    e.setPeriodStart(req.periodStart());
    e.setPeriodEnd(req.periodEnd());
    e.setPlannedAmount(req.plannedAmount());
    e.setCurrency(req.currency());
    return repo.save(e);
  }
}
