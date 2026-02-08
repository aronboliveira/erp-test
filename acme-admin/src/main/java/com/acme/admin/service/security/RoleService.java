package com.acme.admin.service.security;

import com.acme.admin.domain.security.*;
import com.acme.admin.dto.security.RoleDtos.*;
import com.acme.admin.repository.security.*;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.*;

@Service
public class RoleService {
  private final AuthRoleRepository roles;
  private final AuthPermissionRepository perms;

  public RoleService(AuthRoleRepository roles, AuthPermissionRepository perms) {
    this.roles = roles;
    this.perms = perms;
  }

  @Transactional(readOnly = true)
  public List<RoleDto> list() {
    return roles.findAll().stream().map(this::toDto).toList();
  }

  @Transactional
  public RoleDto create(CreateRoleRequest reqRaw) {
    final CreateRoleRequest req = reqRaw.normalized();

    if (roles.existsByName(req.code())) {
      throw new IllegalArgumentException("role.code already exists");
    }

    final Set<AuthPermission> ps = resolvePermissions(req.permissionCodes());

    final AuthRole r = new AuthRole();
    r.setName(req.code());
    r.setDescription(req.title());
    r.setPermissions(ps);
    r.validateForDml();

    return toDto(roles.save(r));
  }

  @Transactional
  public RoleDto update(UUID id, UpdateRoleRequest reqRaw) {
    final UpdateRoleRequest req = reqRaw.normalized();

    final AuthRole r = roles.findById(id)
      .orElseThrow(() -> new NoSuchElementException("role not found"));

    if (!r.getName().equals(req.code()) && roles.existsByName(req.code()))
      throw new IllegalArgumentException("role.code already exists");

    final Set<AuthPermission> ps = resolvePermissions(req.permissionCodes());

    r.setName(req.code());
    r.setDescription(req.title());
    r.setPermissions(ps);
    r.validateForDml();

    return toDto(roles.save(r));
  }

  @Transactional(readOnly = true)
  public AuthRole requireRoleByName(String name) {
    return roles.findByName(name).orElseThrow(() -> new NoSuchElementException("role not found: " + name));
  }

  Set<AuthPermission> resolvePermissions(List<String> codes) {
    final List<String> unique = codes == null ? List.of() : codes.stream().distinct().toList();
    final List<AuthPermission> found = perms.findByCodeIn(unique);
    if (found.size() != unique.size()) throw new IllegalArgumentException("unknown permission code(s)");

    return new LinkedHashSet<>(found);
  }

  RoleDto toDto(AuthRole r) {
    final List<String> codes = r.getPermissions().stream().map(AuthPermission::getCode).sorted().toList();
    return new RoleDto(r.getId(), r.getName(), r.getDescription(), r.getCreatedAt(), codes);
  }
}
