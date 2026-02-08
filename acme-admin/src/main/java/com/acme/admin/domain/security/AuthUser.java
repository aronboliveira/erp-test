package com.acme.admin.domain.security;

import jakarta.persistence.*;
import java.time.Instant;
import java.util.*;

@Entity
@Table(name = "auth_users")
public class AuthUser {
  private static final String RX_EMAIL = "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$";
  private static final String RX_USERNAME = "^[a-zA-Z0-9][a-zA-Z0-9_\\-]{2,59}$";

  public enum Status { ACTIVE, SUSPENDED, DISABLED }

  @Id
  @GeneratedValue
  private UUID id;

  @Column(nullable = false, unique = true, length = 254)
  private String email;

  @Column(nullable = false, unique = true, length = 60)
  private String username;

  @Column(name = "display_name", length = 120)
  private String displayName;

  @Column(name = "password_hash", nullable = false, length = 255)
  private String passwordHash;

  @Enumerated(EnumType.STRING)
  @Column(nullable = false, length = 20)
  private Status status = Status.ACTIVE;

  @Column(name = "last_login_at")
  private Instant lastLoginAt;

  @ManyToMany(fetch = FetchType.LAZY)
  @JoinTable(
    name = "auth_user_roles",
    joinColumns = @JoinColumn(name = "user_id"),
    inverseJoinColumns = @JoinColumn(name = "role_id")
  )
  private Set<AuthRole> roles = new LinkedHashSet<>();

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
    final String e = email == null ? "" : email.trim().toLowerCase();
    final String u = username == null ? "" : username.trim();

    if (!e.matches(RX_EMAIL)) throw new IllegalArgumentException("user.email invalid");
    if (!u.matches(RX_USERNAME)) throw new IllegalArgumentException("user.username invalid");
    if (passwordHash == null || passwordHash.isBlank()) throw new IllegalArgumentException("user.passwordHash required");

    email = e;
    username = u;
  }

  public UUID getId() { return id; }
  public String getEmail() { return email; }
  public String getUsername() { return username; }
  public String getDisplayName() { return displayName; }
  public String getPasswordHash() { return passwordHash; }
  public Status getStatus() { return status; }
  public Instant getLastLoginAt() { return lastLoginAt; }
  public Set<AuthRole> getRoles() { return roles; }
  public Instant getCreatedAt() { return createdAt; }

  public void setEmail(String email) { this.email = email; }
  public void setUsername(String username) { this.username = username; }
  public void setDisplayName(String displayName) { this.displayName = displayName; }
  public void setPasswordHash(String passwordHash) { this.passwordHash = passwordHash; }
  public void setStatus(Status status) { this.status = status; }
  public void setLastLoginAt(Instant lastLoginAt) { this.lastLoginAt = lastLoginAt; }
  public void setRoles(Set<AuthRole> roles) {
    this.roles = roles == null ? new LinkedHashSet<>() : roles;
  }
}
