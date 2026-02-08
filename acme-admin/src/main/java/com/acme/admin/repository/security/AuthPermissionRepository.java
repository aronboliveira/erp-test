package com.acme.admin.repository.security;

import com.acme.admin.domain.security.AuthPermission;
import org.springframework.data.jpa.repository.JpaRepository;
import java.util.*;

public interface AuthPermissionRepository extends JpaRepository<AuthPermission, UUID> {
  Optional<AuthPermission> findByCode(String code);
  List<AuthPermission> findByCodeIn(Collection<String> codes);
}
