package com.acme.admin.service;

import com.acme.admin.domain.ProductOrServiceCategoryEntity;
import com.acme.admin.domain.ProductOrServiceEntity;
import com.acme.admin.dto.ProductOrServiceCreateRequest;
import com.acme.admin.dto.ProductOrServiceResponse;
import com.acme.admin.repository.ProductOrServiceCategoryRepository;
import com.acme.admin.repository.ProductOrServiceRepository;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
public class ProductOrServiceService {
  private final ProductOrServiceRepository repo;
  private final ProductOrServiceCategoryRepository categories;

  public ProductOrServiceService(ProductOrServiceRepository repo, ProductOrServiceCategoryRepository categories) {
    this.repo = repo;
    this.categories = categories;
  }

  public List<ProductOrServiceResponse> list() {
    return repo.findAll().stream().map(this::toResponse).toList();
  }

  public ProductOrServiceResponse create(ProductOrServiceCreateRequest req) {
    final ProductOrServiceCategoryEntity cat = categories.findById(req.categoryId())
      .orElseThrow(() -> new IllegalArgumentException("Category not found: " + req.categoryId()));

    final ProductOrServiceEntity e = new ProductOrServiceEntity();
    e.setKind(req.kind());
    e.setName(req.name());
    e.setSku(req.sku());
    e.setPrice(req.price());
    e.setCurrency(req.currency());
    e.setCategory(cat);

    return toResponse(repo.save(e));
  }

  private ProductOrServiceResponse toResponse(ProductOrServiceEntity e) {
    final UUID categoryId = e.getCategory() != null ? e.getCategory().getId() : null;
    return new ProductOrServiceResponse(
      e.getId(),
      e.getKind(),
      e.getName(),
      e.getSku(),
      e.getPrice().toPlainString(),
      e.getCurrency(),
      categoryId
    );
  }
}
