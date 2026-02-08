package com.acme.admin.repository.security;

import com.acme.admin.domain.security.AuthRole;
import org.springframework.data.jpa.repository.JpaRepository;
import java.util.*;

public interface AuthRoleRepository extends JpaRepository<AuthRole, UUID> {
  Optional<AuthRole> findByName(String name);
  boolean existsByName(String name);
}
