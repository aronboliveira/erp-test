package com.acme.admin.controller;

import com.acme.admin.dto.ProductOrServiceCreateRequest;
import com.acme.admin.dto.ProductOrServiceResponse;
import com.acme.admin.service.ProductOrServiceService;
import jakarta.validation.Valid;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/catalog/items")
public class ProductOrServiceController {
  private final ProductOrServiceService service;

  public ProductOrServiceController(ProductOrServiceService service) {
    this.service = service;
  }

  @GetMapping
  @PreAuthorize("hasAuthority('CATALOG_READ')")
  public List<ProductOrServiceResponse> list() {
    return service.list();
  }

  @PostMapping
  @PreAuthorize("hasAuthority('CATALOG_WRITE')")
  public ProductOrServiceResponse create(@Valid @RequestBody ProductOrServiceCreateRequest req) {
    return service.create(req);
  }
}
