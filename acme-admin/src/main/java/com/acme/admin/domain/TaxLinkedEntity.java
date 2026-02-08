package com.acme.admin.domain;

import com.acme.admin.validation.*;
import jakarta.persistence.Column;
import jakarta.persistence.MappedSuperclass;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.type.SqlTypes;

import java.util.*;

@MappedSuperclass
public abstract class TaxLinkedEntity extends AuditedEntity {
  @JdbcTypeCode(SqlTypes.JSON)
  @Column(name = "tax_ids")
  protected List<UUID> taxIds;

  public List<UUID> getTaxIds() { return taxIds; }
  public void setTaxIds(List<UUID> taxIds) { this.taxIds = taxIds; }

  public ValidationResult validateTaxIdsShape(TaxIdsPolicy policy) {
    final ValidationResult out = new ValidationResult();
    final List<UUID> ids = this.taxIds;

    if (ids == null) return policy.allowEmpty() ? out
      : out.add(new ValidationIssue("taxIds", "taxIds is required (can be empty list)", Severity.ERROR, Map.of()));

    if (ids.isEmpty()) return policy.allowEmpty() ? out
      : out.add(new ValidationIssue("taxIds", "taxIds cannot be empty", Severity.ERROR, Map.of()));

    if (ids.size() > policy.maxItems())
      out.add(new ValidationIssue("taxIds", "taxIds exceeds maxItems", Severity.ERROR, Map.of("maxItems", policy.maxItems())));

    final Set<UUID> seen = new HashSet<>();
    for (UUID id : ids) {
      if (id == null) out.add(new ValidationIssue("taxIds", "taxIds contains null", Severity.ERROR, Map.of()));
      else if (!seen.add(id))
        out.add(new ValidationIssue("taxIds", "taxIds contains duplicates", Severity.WARN, Map.of("taxId", id.toString())));
    }

    return out;
  }
}
