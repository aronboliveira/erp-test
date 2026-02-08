package com.acme.admin.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

public record HiringResponse(
  UUID id,
  String code,
  Instant occurredAt,
  String employeeName,
  String role,
  Instant startAt,
  Instant endAt,
  BigDecimal grossSalary,
  String currency
) {}
