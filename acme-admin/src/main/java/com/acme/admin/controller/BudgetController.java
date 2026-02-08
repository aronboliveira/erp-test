package com.acme.admin.controller;

import com.acme.admin.domain.BudgetEntity;
import com.acme.admin.dto.BudgetCreateRequest;
import com.acme.admin.service.BudgetService;
import jakarta.validation.Valid;
import java.util.List;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/finance/budgets")
public class BudgetController {
  private final BudgetService service;

  public BudgetController(BudgetService service) {
    this.service = service;
  }

  @GetMapping
  public List<BudgetEntity> list() {
    return service.list();
  }

  @PostMapping
  public BudgetEntity create(@Valid @RequestBody BudgetCreateRequest req) {
    return service.create(req);
  }
}
