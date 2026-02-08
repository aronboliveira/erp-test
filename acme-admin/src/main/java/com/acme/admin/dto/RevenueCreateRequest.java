package com.acme.admin.dto;

import jakarta.validation.constraints.*;
import java.math.BigDecimal;
import java.time.Instant;

public record RevenueCreateRequest(
  @NotNull Instant occurredAt,
  @NotNull @DecimalMin("0.00") BigDecimal amount,
  @NotBlank @Size(min = 3, max = 3) String currency,
  String sourceRef
) {}
