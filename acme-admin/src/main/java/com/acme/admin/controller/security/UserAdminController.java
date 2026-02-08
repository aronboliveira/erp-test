package com.acme.admin.controller.security;

import com.acme.admin.dto.security.UserDtos.*;
import com.acme.admin.service.security.UserService;
import jakarta.validation.Valid;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.*;

@RestController
@RequestMapping("/api/admin/users")
public class UserAdminController {
  private final UserService users;

  public UserAdminController(UserService users) {
    this.users = users;
  }

  @GetMapping
  @PreAuthorize("hasAuthority('users.read')")
  public List<UserDto> list() {
    return users.list();
  }

  @PostMapping
  @PreAuthorize("hasAuthority('users.write')")
  public UserDto create(@Valid @RequestBody CreateUserRequest req) {
    return users.create(req);
  }

  @PutMapping("/{id}")
  @PreAuthorize("hasAuthority('users.write')")
  public UserDto update(@PathVariable UUID id, @Valid @RequestBody UpdateUserRequest req) {
    return users.update(id, req);
  }
}
