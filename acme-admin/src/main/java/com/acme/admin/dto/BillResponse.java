package com.acme.admin.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

public record BillResponse(
  UUID id,
  String code,
  Instant occurredAt,
  Instant dueAt,
  String currency,
  BigDecimal total,
  String vendor
) {}
