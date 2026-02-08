package com.acme.admin.service.security;

import com.acme.admin.time.*;
import com.acme.admin.domain.security.*;
import com.acme.admin.dto.security.UserDtos.*;
import com.acme.admin.repository.security.*;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.*;

@Service
public class UserService {
  private final AuthUserRepository users;
  private final RoleService roleService;
  private final PasswordEncoder encoder;
  private final DateMapper dateMapper = new UTCDateMapper();

  public UserService(AuthUserRepository users, RoleService roleService, PasswordEncoder encoder) {
    this.users = users;
    this.roleService = roleService;
    this.encoder = encoder;
  }

  @Transactional(readOnly = true)
  public List<UserDto> list() {
    return users.findAll().stream().map(this::toDto).toList();
  }

  @Transactional
  public UserDto create(CreateUserRequest reqRaw) {
    final CreateUserRequest req = reqRaw.normalized();

    if (users.existsByEmail(req.email())) {
      throw new IllegalArgumentException("user.email already exists");
    }
    if (users.existsByUsername(req.username())) {
      throw new IllegalArgumentException("user.username already exists");
    }

    final Set<AuthRole> rs = resolveRoles(req.roleNames());

    final AuthUser u = new AuthUser();
    u.setEmail(req.email());
    u.setUsername(req.username());
    u.setDisplayName(req.displayName());
    u.setPasswordHash(encoder.encode(req.password()));
    u.setRoles(rs);
    u.validateForDml();

    return toDto(users.save(u));
  }

  @Transactional
  public UserDto update(UUID id, UpdateUserRequest reqRaw) {
    final UpdateUserRequest req = reqRaw.normalized();

    final AuthUser u = users.findById(id)
      .orElseThrow(() -> new NoSuchElementException("user not found"));

    if (!u.getEmail().equals(req.email()) && users.existsByEmail(req.email()))
      throw new IllegalArgumentException("user.email already exists");

    if (!u.getUsername().equals(req.username()) && users.existsByUsername(req.username()))
      throw new IllegalArgumentException("user.username already exists");

    final Set<AuthRole> rs = resolveRoles(req.roleNames());

    final AuthUser.Status st;
    try { st = AuthUser.Status.valueOf(req.status()); }
    catch (Exception e) { throw new IllegalArgumentException("user.status invalid"); }

    u.setEmail(req.email());
    u.setUsername(req.username());
    u.setDisplayName(req.displayName());
    u.setStatus(st);
    u.setRoles(rs);
    u.validateForDml();

    return toDto(users.save(u));
  }

  @Transactional(readOnly = true)
  public ProfileDto profileByUsername(String username) {
    final AuthUser u = users.findByUsername(username)
      .orElseThrow(() -> new NoSuchElementException("user not found"));

    final List<String> roleNames = u.getRoles().stream().map(AuthRole::getName).sorted().toList();
    final Set<String> permCodes = new LinkedHashSet<>();
    for (final AuthRole r : u.getRoles())
      for (final AuthPermission p : r.getPermissions())
        permCodes.add(p.getCode());

    return new ProfileDto(
      u.getId(),
      u.getEmail(),
      u.getUsername(),
      u.getDisplayName(),
      roleNames,
      permCodes.stream().sorted().toList()
    );
  }

  @Transactional
  public void markLogin(UUID userId) {
    final AuthUser u = users.findById(userId)
      .orElseThrow(() -> new NoSuchElementException("user not found"));

    final var now = dateMapper.now();
    DateValidator.requireNotFuture(now, now, "loginAt");

    u.setLastLoginAt(now);
    users.save(u);
  }

  Set<AuthRole> resolveRoles(List<String> names) {
    final List<String> unique = names == null ? List.of() : names.stream().distinct().toList();
    final Set<AuthRole> out = new LinkedHashSet<>();
    for (final String n : unique) out.add(roleService.requireRoleByName(n));
    return out;
  }

  UserDto toDto(AuthUser u) {
    final List<String> roleNames = u.getRoles().stream().map(AuthRole::getName).sorted().toList();
    final Set<String> permCodes = new LinkedHashSet<>();
    for (final AuthRole r : u.getRoles())
      for (final AuthPermission p : r.getPermissions())
        permCodes.add(p.getCode());

    return new UserDto(
      u.getId(),
      u.getEmail(),
      u.getUsername(),
      u.getDisplayName(),
      u.getStatus().name(),
      u.getCreatedAt(),
      roleNames,
      permCodes.stream().sorted().toList()
    );
  }
}
