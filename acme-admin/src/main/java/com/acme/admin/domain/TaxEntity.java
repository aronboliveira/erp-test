package com.acme.admin.domain;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "taxes")
public class TaxEntity extends AuditedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false, length = 64, unique = true)
  private String code;

  @Column(nullable = false, length = 180)
  private String name;

  @Column(nullable = false, precision = 7, scale = 4)
  private BigDecimal rate = BigDecimal.ZERO;

  @Column(nullable = false)
  private boolean enabled = true;

  protected TaxEntity() {}

  public TaxEntity(UUID id, String code, String name, BigDecimal rate) {
      this.id = id;
      this.code = code;
      this.name = name;
      this.rate = rate == null ? BigDecimal.ZERO : rate;
  }
  public UUID getId() { return id; }
  public String getCode() { return code; }
  public void setCode(String code) { this.code = code; }
  public String getName() { return name; }
  public void setName(String name) { this.name = name; }
  public BigDecimal getRate() { return rate; }
  public void setRate(BigDecimal rate) { this.rate = rate; }
  public boolean isEnabled() { return enabled; }
  public void setEnabled(boolean enabled) { this.enabled = enabled; }
}