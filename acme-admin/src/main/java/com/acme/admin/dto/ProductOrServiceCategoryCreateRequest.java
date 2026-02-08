package com.acme.admin.dto;

import jakarta.validation.constraints.*;

public record ProductOrServiceCategoryCreateRequest(
  @NotBlank @Size(max = 64) String code,
  @NotBlank @Size(max = 180) String name,
  String description
) {}
