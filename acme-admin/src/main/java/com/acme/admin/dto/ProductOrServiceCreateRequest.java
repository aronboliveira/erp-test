package com.acme.admin.dto;

import com.acme.admin.domain.ProductKind;
import jakarta.validation.constraints.*;

import java.math.BigDecimal;
import java.util.UUID;

public record ProductOrServiceCreateRequest(
  @NotNull ProductKind kind,
  @NotBlank @Size(max = 180) String name,
  @Size(max = 64) String sku,
  @NotNull @DecimalMin("0.00") BigDecimal price,
  @NotBlank @Size(min = 3, max = 3) String currency,
  @NotNull UUID categoryId
) {}
