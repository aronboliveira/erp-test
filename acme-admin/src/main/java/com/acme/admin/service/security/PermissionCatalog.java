package com.acme.admin.service.security;

import java.util.*;

public final class PermissionCatalog {
  private PermissionCatalog() {}

  public record PermissionSpec(String code, String description) {}

  public static List<PermissionSpec> specs() {
    return List.of(
      new PermissionSpec("users.read", "List/read users"),
      new PermissionSpec("users.write", "Create/update users"),
      new PermissionSpec("roles.read", "List/read roles"),
      new PermissionSpec("roles.write", "Create/update roles"),
      new PermissionSpec("finance.read", "Read finance data"),
      new PermissionSpec("finance.write", "Write finance data")
    );
  }

  public static Set<String> allCodes() {
    final Set<String> out = new LinkedHashSet<>();
    for (final PermissionSpec s : specs()) out.add(s.code());
    return Collections.unmodifiableSet(out);
  }
}
