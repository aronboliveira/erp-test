package com.acme.admin.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

public record PurchaseCreateRequest(
    @NotBlank @Size(max = 64) String code,
    @NotNull Instant occurredAt,
    @NotBlank @Size(min = 3, max = 3) String currency,
    @NotNull BigDecimal total,
    @Size(max = 180) String vendor,
    List<UUID> taxIds
) {}
