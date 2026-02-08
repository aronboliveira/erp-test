package com.acme.admin.dto;

import jakarta.validation.constraints.*;
import java.math.BigDecimal;
import java.time.LocalDate;

public record BudgetCreateRequest(
  @NotNull LocalDate periodStart,
  @NotNull LocalDate periodEnd,
  @NotNull @DecimalMin("0.00") BigDecimal plannedAmount,
  @NotBlank @Size(min = 3, max = 3) String currency
) {}
