package com.acme.admin.validation;

import java.util.List;

public final class ApiValidationException extends RuntimeException {
  private final List<ValidationIssue> issues;

  public ApiValidationException(List<ValidationIssue> issues) {
    super("Validation failed");
    this.issues = issues;
  }

  public List<ValidationIssue> getIssues() {
    return issues;
  }
}
