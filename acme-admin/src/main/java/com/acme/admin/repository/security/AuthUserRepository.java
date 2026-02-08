package com.acme.admin.repository.security;

import com.acme.admin.domain.security.AuthUser;
import org.springframework.data.jpa.repository.EntityGraph;
import org.springframework.data.jpa.repository.JpaRepository;
import java.util.*;

public interface AuthUserRepository extends JpaRepository<AuthUser, UUID> {
  @EntityGraph(attributePaths = {"roles", "roles.permissions"})
  Optional<AuthUser> findByEmail(String email);
  
  @EntityGraph(attributePaths = {"roles", "roles.permissions"})
  Optional<AuthUser> findByUsername(String username);
  
  @EntityGraph(attributePaths = {"roles", "roles.permissions"})
  List<AuthUser> findAll();
  
  boolean existsByEmail(String email);
  boolean existsByUsername(String username);
}
