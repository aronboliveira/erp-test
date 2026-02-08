package com.acme.admin.dto;

import jakarta.validation.constraints.*;
import java.math.BigDecimal;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

public record OrderCreateRequest(
  @NotBlank @Size(max = 64) String code,
  @NotNull Instant occurredAt,
  @NotBlank @Size(min = 3, max = 3) String currency,
  @NotNull @DecimalMin("0.00") BigDecimal total,
  List<UUID> taxIds
) {}
