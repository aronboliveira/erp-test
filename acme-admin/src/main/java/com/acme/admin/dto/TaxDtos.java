package com.acme.admin.dto;

import java.math.BigDecimal;
import java.util.UUID;

public final class TaxDtos {
    private TaxDtos() {}

    public record CreateTaxRequest(
        String code,
        String title,
        BigDecimal rate
    ) {}

    public record UpdateTaxRequest(
        String code,
        String title,
        BigDecimal rate
    ) {}

    public record TaxResponse(
        UUID id,
        String code,
        String title,
        BigDecimal rate,
        String createdAt
    ) {}
}
