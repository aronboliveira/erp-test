package com.acme.admin.dto;

import jakarta.validation.constraints.*;
import java.math.BigDecimal;

public record TaxCreateRequest(
  @NotBlank @Size(max = 64) String code,
  @NotBlank @Size(max = 180) String name,
  @NotNull @DecimalMin("0.0000") BigDecimal rate,
  boolean enabled
) {}
