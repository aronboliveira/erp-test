package com.acme.admin.validation;

import java.util.Map;

public record ValidationIssue(
  String field,
  String reason,
  Severity severity,
  Map<String, Object> meta
) {}
