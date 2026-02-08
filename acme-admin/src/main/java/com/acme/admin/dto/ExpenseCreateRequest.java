package com.acme.admin.dto;

import jakarta.validation.constraints.*;
import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

public record ExpenseCreateRequest(
  @NotNull Instant occurredAt,
  @NotNull @DecimalMin("0.00") BigDecimal amount,
  @NotBlank @Size(min = 3, max = 3) String currency,
  @NotNull UUID categoryId,
  String vendor
) {}
