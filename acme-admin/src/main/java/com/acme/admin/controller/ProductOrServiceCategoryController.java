package com.acme.admin.controller;

import com.acme.admin.domain.ProductOrServiceCategoryEntity;
import com.acme.admin.dto.ProductOrServiceCategoryCreateRequest;
import com.acme.admin.repository.ProductOrServiceCategoryRepository;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/catalog/categories")
public class ProductOrServiceCategoryController {
  private final ProductOrServiceCategoryRepository repo;

  public ProductOrServiceCategoryController(ProductOrServiceCategoryRepository repo) {
    this.repo = repo;
  }

  @GetMapping
  public List<ProductOrServiceCategoryEntity> list() {
    return repo.findAll();
  }

  @PostMapping
  public ProductOrServiceCategoryEntity create(@Valid @RequestBody ProductOrServiceCategoryCreateRequest req) {
    final ProductOrServiceCategoryEntity e = new ProductOrServiceCategoryEntity();
    e.setCode(req.code());
    e.setName(req.name());
    e.setDescription(req.description());
    return repo.save(e);
  }
}
