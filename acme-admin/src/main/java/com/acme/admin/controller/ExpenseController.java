package com.acme.admin.controller;

import com.acme.admin.domain.ExpenseCategoryEntity;
import com.acme.admin.domain.ExpenseEntity;
import com.acme.admin.dto.ExpenseCreateRequest;
import com.acme.admin.dto.ExpenseResponse;
import com.acme.admin.repository.ExpenseCategoryRepository;
import com.acme.admin.service.ExpenseService;
import com.acme.admin.time.DateMapper;
import com.acme.admin.time.UTCDateMapper;
import com.acme.admin.validation.*;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.time.*;
import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/finance/expenses")
public class ExpenseController {
  private final ExpenseService service;
  private final ExpenseCategoryRepository catRepo;

  private final Clock clock = Clock.systemUTC();
  private final DateMapper dateMapper = new UTCDateMapper();
  private final OccurredAtPolicy occurredAtPolicy =
    new OccurredAtPolicy(false, Duration.ofDays(3650), Duration.ofMinutes(5));

  public ExpenseController(ExpenseService service, ExpenseCategoryRepository catRepo) {
    this.service = service;
    this.catRepo = catRepo;
  }

  @GetMapping
  public List<ExpenseResponse> list() {
    return service.list();
  }

  @PostMapping
  public ExpenseResponse create(@Valid @RequestBody ExpenseCreateRequest req) {
    final ExpenseCategoryEntity cat = catRepo.findById(req.categoryId())
      .orElseThrow(() -> new ApiValidationException(List.of(
        new ValidationIssue("categoryId", "Expense category not found", Severity.ERROR, Map.of("categoryId", req.categoryId().toString()))
      )));

    final ExpenseEntity e = new ExpenseEntity();
    e.setOccurredAt(req.occurredAt());
    e.setAmount(req.amount());
    e.setCurrency(req.currency());
    e.setVendor(req.vendor());
    e.setCategory(cat);

    final ValidationResult r = e.validateForUpsert(clock, dateMapper, occurredAtPolicy);
    if (r.hasErrors()) throw new ApiValidationException(r.toList());

    return service.create(req);
  }
}
