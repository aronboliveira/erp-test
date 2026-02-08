package com.acme.admin.validation;

import java.util.*;

public final class ValidationResult {
  private final List<ValidationIssue> issues = new ArrayList<>();

  public ValidationResult add(ValidationIssue i) {
    if (i != null) issues.add(i);
    return this;
  }

  public ValidationResult addAll(Collection<ValidationIssue> list) {
    if (list == null || list.isEmpty()) return this;
    for (ValidationIssue i : list) add(i);
    return this;
  }

  public boolean hasErrors() {
    for (ValidationIssue i : issues)
      if (i != null && i.severity() == Severity.ERROR) return true;
    return false;
  }

  public List<ValidationIssue> toList() {
    return List.copyOf(issues);
  }
}
