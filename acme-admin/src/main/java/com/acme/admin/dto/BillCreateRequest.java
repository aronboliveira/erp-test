package com.acme.admin.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

public record BillCreateRequest(
  String code,
  Instant occurredAt,
  Instant dueAt,
  String currency,
  BigDecimal total,
  String vendor,
  String payee,
  List<UUID> taxIds
) {}
