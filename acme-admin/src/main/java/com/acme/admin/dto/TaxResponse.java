package com.acme.admin.dto;

import java.util.UUID;

public record TaxResponse(
  UUID id,
  String code,
  String name,
  String rate,
  boolean enabled
) {}
