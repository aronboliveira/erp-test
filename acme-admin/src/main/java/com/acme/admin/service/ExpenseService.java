package com.acme.admin.service;

import com.acme.admin.domain.ExpenseCategoryEntity;
import com.acme.admin.domain.ExpenseEntity;
import com.acme.admin.dto.ExpenseCreateRequest;
import com.acme.admin.dto.ExpenseResponse;
import com.acme.admin.repository.ExpenseCategoryRepository;
import com.acme.admin.repository.ExpenseRepository;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
public class ExpenseService {
  private final ExpenseRepository repo;
  private final ExpenseCategoryRepository categories;

  public ExpenseService(ExpenseRepository repo, ExpenseCategoryRepository categories) {
    this.repo = repo;
    this.categories = categories;
  }

  public List<ExpenseResponse> list() {
    return repo.findAll().stream().map(this::toResponse).toList();
  }

  public ExpenseResponse create(ExpenseCreateRequest req) {
    final ExpenseCategoryEntity cat = categories.findById(req.categoryId())
      .orElseThrow(() -> new IllegalArgumentException("Expense category not found: " + req.categoryId()));

    final ExpenseEntity e = new ExpenseEntity();
    e.setOccurredAt(req.occurredAt());
    e.setAmount(req.amount());
    e.setCurrency(req.currency());
    e.setVendor(req.vendor());
    e.setCategory(cat);

    return toResponse(repo.save(e));
  }

  private ExpenseResponse toResponse(ExpenseEntity e) {
    final UUID categoryId = e.getCategory() != null ? e.getCategory().getId() : null;
    return new ExpenseResponse(
      e.getId(),
      e.getOccurredAt().toString(),
      e.getAmount().toPlainString(),
      e.getCurrency(),
      categoryId,
      e.getVendor()
    );
  }
}
