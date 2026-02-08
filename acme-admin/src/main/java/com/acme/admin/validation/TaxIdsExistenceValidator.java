package com.acme.admin.validation;

import com.acme.admin.repository.TaxRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import java.util.*;

@Component
public final class TaxIdsExistenceValidator implements IdsExistenceValidator<UUID> {
  private final TaxRepository repo;
  private final Logger log = LoggerFactory.getLogger(TaxIdsExistenceValidator.class);

  public TaxIdsExistenceValidator(TaxRepository repo) {
    this.repo = repo;
  }

  @Override
  public ValidationResult validate(String field, Collection<UUID> ids, ValidationResult acc) {
    acc = acc != null ? acc : new ValidationResult();
    if (ids == null || ids.isEmpty()) return acc;

    final Set<UUID> requested = new HashSet<>();
    for (UUID id : ids) {
      if (id != null) {
        requested.add(id);
      } else {
        acc.add(new ValidationIssue(field, "Contains null id", Severity.ERROR, Map.of()));
      }
    }

    if (requested.isEmpty()) return acc;

    try {
      final Set<UUID> found = new HashSet<>();
      for (var t : repo.findAllById(requested)) {
        if (t == null || t.getId() == null) continue;
        found.add(t.getId());
      }

      if (found.size() == requested.size()) return acc;

      for (UUID id : requested)
        if (!found.contains(id))
          acc.add(new ValidationIssue(
            field,
            "Unknown id",
            Severity.ERROR,
            Map.of("id", id.toString())
          ));
    } catch (RuntimeException e) {
      log.error("Failed to validate {} existence", field, e);
      acc.add(new ValidationIssue(
        field,
        "Could not validate id existence (repository failure)",
        Severity.ERROR,
        Map.of("reason", e.getClass().getSimpleName())
      ));
    }

    return acc;
  }
}
