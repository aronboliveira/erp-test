package com.acme.admin.domain;

import com.acme.admin.time.DateMapper;
import com.acme.admin.time.DateValidator;
import com.acme.admin.validation.*;
import jakarta.persistence.*;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.annotations.UuidGenerator;
import org.hibernate.type.SqlTypes;

import java.math.BigDecimal;
import java.time.*;
import java.util.List;
import java.util.Map;
import java.util.UUID;

@Entity
@Table(name = "orders")
public class OrderEntity extends TaxLinkedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false, length = 64, unique = true)
  private String code;

  @Column(nullable = false)
  private Instant occurredAt;

  @Column(name = "issued_at", nullable = false)
  private OffsetDateTime issuedAt = OffsetDateTime.now();

  @Column(nullable = false, length = 3)
  private String currency;

  @Column(nullable = false, precision = 15, scale = 2)
  private BigDecimal total;

  @JdbcTypeCode(SqlTypes.JSON)
  @Column(name = "tax_ids")
  private List<UUID> taxIds;

  public OrderEntity() {}

  public OrderEntity(UUID id, String code, OffsetDateTime issuedAt, Instant occurredAt, String currency, BigDecimal total) {
      this.id = id;
      this.code = code;
      this.issuedAt = issuedAt == null ? OffsetDateTime.now() : issuedAt;
      this.occurredAt = occurredAt == null ? Instant.now() : occurredAt;
      this.currency = currency;
      this.total = total;
  }

  public UUID getId() { return id; }
  public String getCode() { return code; }
  public void setCode(String code) { this.code = code; }
  public Instant getOccurredAt() { return occurredAt; }
  public void setOccurredAt(Instant occurredAt) { this.occurredAt = occurredAt; }
  public String getCurrency() { return currency; }
  public void setCurrency(String currency) { this.currency = currency; }
  public BigDecimal getTotal() { return total; }
  public void setTotal(BigDecimal total) { this.total = total; }

  public List<UUID> getTaxIds() {
      return taxIds == null ? null : List.copyOf(taxIds);
  }

  public void applyTaxIdsNormalized(List<UUID> normalized) {
      this.taxIds = normalized;
  }

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
