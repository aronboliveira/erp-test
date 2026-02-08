package com.acme.admin.domain;

import com.acme.admin.time.DateMapper;
import com.acme.admin.time.DateValidator;
import com.acme.admin.validation.*;
import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.math.BigDecimal;
import java.time.Clock;
import java.time.Instant;
import java.time.LocalDate;
import java.util.Map;
import java.util.UUID;

@Entity
@Table(name = "purchases")
public class PurchaseEntity extends TaxLinkedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false, length = 64, unique = true)
  private String code;

  @Column(nullable = false)
  private Instant occurredAt;

  @Column(nullable = false, length = 3)
  private String currency;

  @Column(nullable = false, precision = 15, scale = 2)
  private BigDecimal total;

  @Column(length = 180)
  private String vendor;

  public UUID getId() { return id; }
  public String getCode() { return code; }
  public void setCode(String code) { this.code = code; }
  public Instant getOccurredAt() { return occurredAt; }
  public void setOccurredAt(Instant occurredAt) { this.occurredAt = occurredAt; }
  public String getCurrency() { return currency; }
  public void setCurrency(String currency) { this.currency = currency; }
  public BigDecimal getTotal() { return total; }
  public void setTotal(BigDecimal total) { this.total = total; }
  public String getVendor() { return vendor; }
  public void setVendor(String vendor) { this.vendor = vendor; }

  public ValidationResult validateForUpsert(
    Clock clock,
    DateMapper mapper,
    OccurredAtPolicy occurredAtPolicy,
    TaxIdsPolicy taxPolicy
  ) {
    final ValidationResult out = new ValidationResult();

    if (code == null || code.isBlank())
      out.add(new ValidationIssue("code", "code is required", Severity.ERROR, Map.of()));

    if (currency == null || currency.length() != 3)
      out.add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, Map.of()));

    if (total == null || total.signum() < 0)
      out.add(new ValidationIssue("total", "total must be >= 0", Severity.ERROR, Map.of()));

    if (vendor != null && vendor.length() > 180)
      out.add(new ValidationIssue("vendor", "vendor must be <= 180 chars", Severity.ERROR, Map.of("maxLen", 180)));

    if (occurredAt == null)
      out.add(new ValidationIssue("occurredAt", "occurredAt is required", Severity.ERROR, Map.of()));
    else {
      if (DateValidator.isFuture(occurredAt, clock, occurredAtPolicy.futureSkew()))
        out.add(new ValidationIssue("occurredAt", "occurredAt cannot be in the future", Severity.ERROR, Map.of()));

      if (DateValidator.isTooOld(occurredAt, clock, occurredAtPolicy.maxAge()))
        out.add(new ValidationIssue("occurredAt", "occurredAt exceeds maxAge", Severity.ERROR, Map.of("maxAge", occurredAtPolicy.maxAge().toString())));

      if (occurredAtPolicy.businessDaysOnly()) {
        final LocalDate d = DateValidator.toUtcDate(occurredAt);
        if (!mapper.isBusinessDay(d))
          out.add(new ValidationIssue("occurredAt", "occurredAt must be a business day (Mon–Fri)", Severity.WARN, Map.of("date", d.toString())));
      }
    }

    return out.addAll(validateTaxIdsShape(taxPolicy).toList());
  }
}
