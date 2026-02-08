package com.acme.admin.domain.security;

import jakarta.persistence.*;
import java.time.Instant;
import java.util.*;

@Entity
@Table(name = "auth_roles")
public class AuthRole {
  private static final String RX_NAME = "^[a-zA-Z0-9][a-zA-Z0-9_\\- ]{2,59}$";

  @Id
  @GeneratedValue
  private UUID id;

  @Column(nullable = false, unique = true, length = 60)
  private String name;

  @Column(columnDefinition = "text")
  private String description;

  @ManyToMany(fetch = FetchType.LAZY)
  @JoinTable(
    name = "auth_role_permissions",
    joinColumns = @JoinColumn(name = "role_id"),
    inverseJoinColumns = @JoinColumn(name = "permission_id")
  )
  private Set<AuthPermission> permissions = new LinkedHashSet<>();

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
    final String n = name == null ? "" : name.trim();
    if (!n.matches(RX_NAME)) throw new IllegalArgumentException("role.name invalid");
    name = n;
  }

  public UUID getId() { return id; }
  public String getName() { return name; }
  public String getDescription() { return description; }
  public Set<AuthPermission> getPermissions() { return permissions; }
  public Instant getCreatedAt() { return createdAt; }
  public Instant getUpdatedAt() { return updatedAt; }

  public void setName(String name) { this.name = name; }
  public void setDescription(String description) { this.description = description; }
  public void setPermissions(Set<AuthPermission> permissions) {
    this.permissions = permissions == null ? new LinkedHashSet<>() : permissions;
  }
}
