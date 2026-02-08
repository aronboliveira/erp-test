package com.acme.admin.domain;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.util.UUID;

@Entity
@Table(name = "expense_categories")
public class ExpenseCategoryEntity extends AuditedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Column(nullable = false, length = 64, unique = true)
  private String code;

  @Column(nullable = false, length = 180)
  private String name;

  @Enumerated(EnumType.STRING)
  @Column(nullable = false, length = 32)
  private ExpenseSubject subject;

  @Column(columnDefinition = "text")
  private String description;

  public UUID getId() { return id; }
  public String getCode() { return code; }
  public void setCode(String code) { this.code = code; }
  public String getName() { return name; }
  public void setName(String name) { this.name = name; }
  public ExpenseSubject getSubject() { return subject; }
  public void setSubject(ExpenseSubject subject) { this.subject = subject; }
  public String getDescription() { return description; }
  public void setDescription(String description) { this.description = description; }
}
