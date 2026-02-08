package com.acme.admin.dto;

import com.acme.admin.domain.ProductKind;
import java.util.UUID;

public record ProductOrServiceResponse(
  UUID id,
  ProductKind kind,
  String name,
  String sku,
  String price,
  String currency,
  UUID categoryId
) {}
