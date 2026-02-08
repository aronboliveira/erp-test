package com.acme.admin.controller.security;

import com.acme.admin.dto.security.RoleDtos.*;
import com.acme.admin.service.security.RoleService;
import jakarta.validation.Valid;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.*;

@RestController
@RequestMapping("/api/admin/roles")
public class RoleAdminController {
  private final RoleService roles;

  public RoleAdminController(RoleService roles) {
    this.roles = roles;
  }

  @GetMapping
  @PreAuthorize("hasAuthority('roles.read')")
  public List<RoleDto> list() {
    return roles.list();
  }

  @PostMapping
  @PreAuthorize("hasAuthority('roles.write')")
  public RoleDto create(@Valid @RequestBody CreateRoleRequest req) {
    return roles.create(req);
  }

  @PutMapping("/{id}")
  @PreAuthorize("hasAuthority('roles.write')")
  public RoleDto update(@PathVariable UUID id, @Valid @RequestBody UpdateRoleRequest req) {
    return roles.update(id, req);
  }
}
