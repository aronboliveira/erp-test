package com.acme.admin.domain;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.UUID;

@Entity
@Table(name = "budgets")
public class BudgetEntity extends AuditedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false)
  private LocalDate periodStart;

  @Column(nullable = false)
  private LocalDate periodEnd;

  @Column(nullable = false, precision = 15, scale = 2)
  private BigDecimal plannedAmount;

  @Column(nullable = false, length = 3)
  private String currency;

  public UUID getId() { return id; }
  public LocalDate getPeriodStart() { return periodStart; }
  public void setPeriodStart(LocalDate periodStart) { this.periodStart = periodStart; }
  public LocalDate getPeriodEnd() { return periodEnd; }
  public void setPeriodEnd(LocalDate periodEnd) { this.periodEnd = periodEnd; }
  public BigDecimal getPlannedAmount() { return plannedAmount; }
  public void setPlannedAmount(BigDecimal plannedAmount) { this.plannedAmount = plannedAmount; }
  public String getCurrency() { return currency; }
  public void setCurrency(String currency) { this.currency = currency; }
}
