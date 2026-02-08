package com.acme.admin.dto;

import com.acme.admin.domain.ExpenseSubject;
import jakarta.validation.constraints.*;

public record ExpenseCategoryCreateRequest(
  @NotBlank @Size(max = 64) String code,
  @NotBlank @Size(max = 180) String name,
  @NotNull ExpenseSubject subject,
  String description
) {}
