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
@Table(name = "hirings")
public class HiringEntity extends TaxLinkedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false, length = 64, unique = true)
  private String code;

  /**
   * Hiring event timestamp (same semantics as OrderEntity.occurredAt).
   */
  @Column(nullable = false)
  private Instant occurredAt;

  @Column(nullable = false, length = 180)
  private String employeeName;

  @Column(nullable = false, length = 120)
  private String role;

  @Column(nullable = false)
  private Instant startAt;

  @Column
  private Instant endAt;

  @Column(nullable = false, precision = 15, scale = 2)
  private BigDecimal grossSalary;

  @Column(nullable = false, length = 3)
  private String currency;

  @Column(precision = 15, scale = 2)
  private BigDecimal total;

  @Column(length = 180)
  private String candidateName;

  public UUID getId() { return id; }

  public String getCode() { return code; }
  public void setCode(String code) { this.code = code; }

  public Instant getOccurredAt() { return occurredAt; }
  public void setOccurredAt(Instant occurredAt) { this.occurredAt = occurredAt; }

  public String getEmployeeName() { return employeeName; }
  public void setEmployeeName(String employeeName) { this.employeeName = employeeName; }

  public String getRole() { return role; }
  public void setRole(String role) { this.role = role; }

  public Instant getStartAt() { return startAt; }
  public void setStartAt(Instant startAt) { this.startAt = startAt; }

  public Instant getEndAt() { return endAt; }
  public void setEndAt(Instant endAt) { this.endAt = endAt; }

  public BigDecimal getGrossSalary() { return grossSalary; }
  public void setGrossSalary(BigDecimal grossSalary) { this.grossSalary = grossSalary; }

  public String getCurrency() { return currency; }
  public void setCurrency(String currency) { this.currency = currency; }

  public BigDecimal getTotal() { return total; }
  public void setTotal(BigDecimal total) { this.total = total; }

  public String getCandidateName() { return candidateName; }
  public void setCandidateName(String candidateName) { this.candidateName = candidateName; }

  public ValidationResult validateForUpsert(
    Clock clock,
    DateMapper mapper,
    OccurredAtPolicy occurredAtPolicy,
    TaxIdsPolicy taxPolicy
  ) {
    final ValidationResult out = new ValidationResult();

    if (code == null || code.isBlank())
      out.add(new ValidationIssue("code", "code is required", Severity.ERROR, Map.of()));

    if (employeeName == null || employeeName.isBlank())
      out.add(new ValidationIssue("employeeName", "employeeName is required", Severity.ERROR, Map.of()));
    else if (employeeName.length() > 180)
      out.add(new ValidationIssue("employeeName", "employeeName must be <= 180 chars", Severity.ERROR, Map.of("maxLen", 180)));

    if (role == null || role.isBlank())
      out.add(new ValidationIssue("role", "role is required", Severity.ERROR, Map.of()));
    else if (role.length() > 120)
      out.add(new ValidationIssue("role", "role must be <= 120 chars", Severity.ERROR, Map.of("maxLen", 120)));

    if (currency == null || currency.length() != 3)
      out.add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, Map.of()));

    if (grossSalary == null || grossSalary.signum() < 0)
      out.add(new ValidationIssue("grossSalary", "grossSalary must be >= 0", Severity.ERROR, Map.of()));

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


    if (startAt == null) {
      out.add(new ValidationIssue("startAt", "startAt is required", Severity.ERROR, Map.of()));
    } else if (occurredAt != null && startAt.isBefore(occurredAt)) {
      out.add(new ValidationIssue("startAt", "startAt cannot be before occurredAt", Severity.ERROR,
        Map.of("startAt", startAt.toString(), "occurredAt", occurredAt.toString())));
    }

    if (endAt != null && startAt != null && endAt.isBefore(startAt)) {
      out.add(new ValidationIssue("endAt", "endAt cannot be before startAt", Severity.ERROR,
        Map.of("endAt", endAt.toString(), "startAt", startAt.toString())));
    }

    return out.addAll(validateTaxIdsShape(taxPolicy).toList());
  }
}
