package com.acme.admin.controller;

import com.acme.admin.domain.RevenueEntity;
import com.acme.admin.dto.RevenueCreateRequest;
import com.acme.admin.service.RevenueService;
import jakarta.validation.Valid;
import java.util.List;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/finance/revenue")
public class RevenueController {
  private final RevenueService service;

  public RevenueController(RevenueService service) {
    this.service = service;
  }

  @GetMapping
  public List<RevenueEntity> list() {
    return service.list();
  }

  @PostMapping
  public RevenueEntity create(@Valid @RequestBody RevenueCreateRequest req) {
    return service.create(req);
  }
}
