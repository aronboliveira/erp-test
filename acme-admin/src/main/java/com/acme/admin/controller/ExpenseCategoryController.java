package com.acme.admin.controller;

import com.acme.admin.domain.ExpenseCategoryEntity;
import com.acme.admin.dto.ExpenseCategoryCreateRequest;
import com.acme.admin.repository.ExpenseCategoryRepository;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/finance/expense-categories")
public class ExpenseCategoryController {
  private final ExpenseCategoryRepository repo;

  public ExpenseCategoryController(ExpenseCategoryRepository repo) {
    this.repo = repo;
  }

  @GetMapping
  public List<ExpenseCategoryEntity> list() {
    return repo.findAll();
  }

  @PostMapping
  public ExpenseCategoryEntity create(@Valid @RequestBody ExpenseCategoryCreateRequest req) {
    final ExpenseCategoryEntity e = new ExpenseCategoryEntity();
    e.setCode(req.code());
    e.setName(req.name());
    e.setSubject(req.subject());
    e.setDescription(req.description());
    return repo.save(e);
  }
}
