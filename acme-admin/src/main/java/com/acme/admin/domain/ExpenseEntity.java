package com.acme.admin.domain;

import com.acme.admin.time.DateMapper;
import com.acme.admin.time.DateValidator;
import com.acme.admin.validation.*;
import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.math.BigDecimal;
import java.time.*;
import java.util.Map;
import java.util.UUID;

@Entity
@Table(name = "expenses")
public class ExpenseEntity extends AuditedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false)
  private Instant occurredAt;

  @Column(nullable = false, precision = 15, scale = 2)
  private BigDecimal amount;

  @Column(nullable = false, length = 3)
  private String currency;

  @ManyToOne(optional = false, fetch = FetchType.LAZY)
  @JoinColumn(name = "category_id", nullable = false)
  private ExpenseCategoryEntity category;

  @Column(length = 180)
  private String vendor;

  public UUID getId() { return id; }
  public Instant getOccurredAt() { return occurredAt; }
  public void setOccurredAt(Instant occurredAt) { this.occurredAt = occurredAt; }
  public BigDecimal getAmount() { return amount; }
  public void setAmount(BigDecimal amount) { this.amount = amount; }
  public String getCurrency() { return currency; }
  public void setCurrency(String currency) { this.currency = currency; }
  public ExpenseCategoryEntity getCategory() { return category; }
  public void setCategory(ExpenseCategoryEntity category) { this.category = category; }
  public String getVendor() { return vendor; }
  public void setVendor(String vendor) { this.vendor = vendor; }

  public ValidationResult validateForUpsert(
    Clock clock,
    DateMapper mapper,
    OccurredAtPolicy occurredAtPolicy
  ) {
    final ValidationResult out = new ValidationResult();

    if (currency == null || currency.length() != 3)
      out.add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, Map.of()));

    if (amount == null || amount.signum() < 0)
      out.add(new ValidationIssue("amount", "amount must be >= 0", Severity.ERROR, Map.of()));

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

    return out;
  }
}
