package com.acme.admin.domain;

import jakarta.persistence.*;
import java.time.Instant;

@MappedSuperclass
public abstract class AuditedEntity {
  @Column(nullable = false)
  protected Instant createdAt;

  @Column(nullable = false)
  protected Instant updatedAt;

  @PrePersist
  protected void onCreate() {
    final Instant now = Instant.now();
    createdAt = now;
    updatedAt = now;
  }

  @PreUpdate
  protected void onUpdate() {
    updatedAt = Instant.now();
  }

  public Instant getCreatedAt() { return createdAt; }
  public Instant getUpdatedAt() { return updatedAt; }
}
