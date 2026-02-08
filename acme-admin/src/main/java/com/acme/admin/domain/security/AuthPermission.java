package com.acme.admin.domain.security;

import jakarta.persistence.*;
import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "auth_permissions")
public class AuthPermission {
  private static final String RX_CODE = "^[a-z0-9]+(\\.[a-z0-9]+)*$";

  @Id
  @GeneratedValue
  private UUID id;

  @Column(nullable = false, unique = true, length = 80)
  private String code;

  @Column(columnDefinition = "text")
  private String description;

  @Column(name = "created_at", nullable = false)
  private Instant createdAt;

  @Column(name = "updated_at", nullable = false)
  private Instant updatedAt;

  @PrePersist
  void prePersist() {
    createdAt = createdAt == null ? Instant.now() : createdAt;
    updatedAt = updatedAt == null ? createdAt : updatedAt;
  }

  @PreUpdate
  void preUpdate() {
    updatedAt = Instant.now();
  }

  public void validateForDml() {
    final String c = code == null ? "" : code.trim();
    if (!c.matches(RX_CODE)) throw new IllegalArgumentException("permission.code invalid");
    code = c;
  }

  public UUID getId() { return id; }
  public String getCode() { return code; }
  public String getDescription() { return description; }

  public void setCode(String code) { this.code = code; }
  public void setDescription(String description) { this.description = description; }
}
