package com.acme.admin.dto;

import java.util.UUID;

public record ExpenseResponse(
  UUID id,
  String occurredAt,
  String amount,
  String currency,
  UUID categoryId,
  String vendor
) {}
