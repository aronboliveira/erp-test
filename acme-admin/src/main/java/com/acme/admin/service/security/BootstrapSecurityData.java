package com.acme.admin.service.security;

import com.acme.admin.domain.security.*;
import com.acme.admin.service.security.*;
import com.acme.admin.repository.security.*;
import org.springframework.boot.CommandLineRunner;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;

import java.util.*;

@Component
public class BootstrapSecurityData implements CommandLineRunner {
  private final AuthPermissionRepository perms;
  private final AuthRoleRepository roles;
  private final AuthUserRepository users;
  private final PasswordEncoder encoder;

  public BootstrapSecurityData(
    AuthPermissionRepository perms,
    AuthRoleRepository roles,
    AuthUserRepository users,
    PasswordEncoder encoder
  ) {
    this.perms = perms;
    this.roles = roles;
    this.users = users;
    this.encoder = encoder;
  }

  @Override
  public void run(String... args) {
    seedPermissions();
    seedSuperAdminRole();
  }

  void seedPermissions() {
    for (final PermissionCatalog.PermissionSpec spec : PermissionCatalog.specs()) {
      perms.findByCode(spec.code()).orElseGet(() -> {
        final AuthPermission p = new AuthPermission();
        p.setCode(spec.code());
        p.setDescription(spec.description());
        p.validateForDml();
        return perms.save(p);
      });
    }
  }

  void seedSuperAdminRole() {
    roles.findByName("SuperAdmin").orElseGet(() -> {
      final AuthRole r = new AuthRole();
      r.setName("SuperAdmin");
      r.setDescription("Full access");
      r.validateForDml();

      final List<AuthPermission> all = perms.findByCodeIn(PermissionCatalog.allCodes());
      r.setPermissions(new LinkedHashSet<>(all));
      return roles.save(r);
    });
  }
}
