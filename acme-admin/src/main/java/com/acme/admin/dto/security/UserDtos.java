package com.acme.admin.dto.security;

import jakarta.validation.constraints.*;
import java.time.Instant;
import java.util.*;

public final class UserDtos {
  private UserDtos() {}

  public record UserDto(
    UUID id,
    String email,
    String username,
    String displayName,
    String status,
    Instant createdAt,
    List<String> roleNames,
    List<String> permissionCodes
  ) {}

  public record CreateUserRequest(
    @NotBlank @Email @Size(max = 254) String email,
    @NotBlank @Size(min = 3, max = 60) String username,
    @Size(max = 120) String displayName,
    @NotBlank @Size(min = 8, max = 120) String password,
    @NotNull List<@NotBlank String> roleNames
  ) {
    public CreateUserRequest normalized() {
      final String e = email == null ? null : email.trim().toLowerCase();
      final String u = username == null ? null : username.trim();
      final String dn = displayName == null ? null : displayName.trim();
      final String p = password == null ? null : password;
      final List<String> rn = roleNames == null
        ? List.of()
        : roleNames.stream().filter(Objects::nonNull).map(String::trim).filter(s -> !s.isBlank()).toList();

      return new CreateUserRequest(e, u, dn, p, rn);
    }
  }

  public record UpdateUserRequest(
    @NotBlank @Email @Size(max = 254) String email,
    @NotBlank @Size(min = 3, max = 60) String username,
    @Size(max = 120) String displayName,
    @NotBlank @Size(max = 20) String status,
    @NotNull List<@NotBlank String> roleNames
  ) {
    public UpdateUserRequest normalized() {
      final String e = email == null ? null : email.trim().toLowerCase();
      final String u = username == null ? null : username.trim();
      final String dn = displayName == null ? null : displayName.trim();
      final String st = status == null ? null : status.trim().toUpperCase();
      final List<String> rn = roleNames == null
        ? List.of()
        : roleNames.stream().filter(Objects::nonNull).map(String::trim).filter(s -> !s.isBlank()).toList();

      return new UpdateUserRequest(e, u, dn, st, rn);
    }
  }

  public record ProfileDto(
    UUID id,
    String email,
    String username,
    String displayName,
    List<String> roleNames,
    List<String> permissionCodes
  ) {}
}
