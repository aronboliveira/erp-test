package com.acme.admin.validation;

public record TaxIdsPolicy(
  int maxItems,
  boolean allowEmpty
) {}
