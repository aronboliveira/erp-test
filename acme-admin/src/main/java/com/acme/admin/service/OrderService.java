package com.acme.admin.service;

import com.acme.admin.domain.OrderEntity;
import com.acme.admin.dto.OrderCreateRequest;
import com.acme.admin.dto.OrderDtos;
import com.acme.admin.repository.OrderRepository;
import com.acme.admin.repository.TaxRepository;
import com.acme.admin.validation.DbSchemaIntrospector;
import com.acme.admin.validation.TaxIdsExistencePort;
import com.acme.admin.validation.TaxIdsValidator;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.time.OffsetDateTime;
import java.time.format.DateTimeParseException;
import java.util.List;
import java.util.UUID;

@Service
public class OrderService {
  private final OrderRepository repo;
  private final TaxIdNormalizer normalizer;
  private final TaxIdsValidator taxIds;

  public OrderService(OrderRepository repo, TaxIdNormalizer normalizer, TaxRepository taxes, DbSchemaIntrospector schema) {
    this.repo = repo;
    this.normalizer = normalizer;
    final TaxIdsExistencePort port = (ids) -> taxes.countByIdIn(ids);
    this.taxIds = new TaxIdsValidator(port, schema, "orders", "tax_ids");
  }

  public List<OrderEntity> list() {
    return repo.findAll();
  }

  public OrderEntity create(OrderCreateRequest req) {
    final OrderEntity e = new OrderEntity();
    e.setCode(req.code());
    e.setOccurredAt(req.occurredAt());
    e.setCurrency(req.currency());
    e.setTotal(req.total());
    e.setTaxIds(normalizer.normalize(req.taxIds()));
    return repo.save(e);
  }

  @Transactional
  public OrderEntity create(OrderDtos.CreateOrderRequest req) {
      final UUID id = UUID.randomUUID();
      final OffsetDateTime issuedAt = parseIssuedAt(req.issuedAt());
      final OrderEntity o = new OrderEntity(id, req.code(), issuedAt, Instant.now(), "USD", null);
      o.applyTaxIdsNormalized(req.taxIds());
      return repo.save(o);
  }

  private OffsetDateTime parseIssuedAt(String raw) {
      if (raw == null || raw.isBlank()) return OffsetDateTime.now();
      try {
          return OffsetDateTime.parse(raw);
      } catch (DateTimeParseException e) {
          return OffsetDateTime.now();
      }
  }
}
