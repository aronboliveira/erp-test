package com.acme.admin.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

public record HiringCreateRequest(
  String code,
  Instant occurredAt,
  String employeeName,
  String role,
  Instant startAt,
  Instant endAt,
  BigDecimal grossSalary,
  String currency,
  BigDecimal total,
  String candidateName,
  List<UUID> taxIds
) {}
