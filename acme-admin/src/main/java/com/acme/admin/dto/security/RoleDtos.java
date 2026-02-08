package com.acme.admin.dto.security;

import jakarta.validation.constraints.*;
import java.time.Instant;
import java.util.*;

public final class RoleDtos {
    private RoleDtos() {}

    public record RoleDto(
        UUID id,
        String code,
        String title,
        Instant createdAt,
        List<String> permissionCodes
    ) {}

    public record CreateRoleRequest(
        @NotBlank @Size(max = 60) String code,
        @NotBlank @Size(max = 120) String title,
        @NotNull List<@NotBlank String> permissionCodes
    ) {
        public CreateRoleRequest normalized() {
            final String c = code == null ? null : code.trim().toUpperCase();
            final String t = title == null ? null : title.trim();
            final List<String> pc = permissionCodes == null
                ? List.of()
                : permissionCodes.stream().filter(Objects::nonNull).map(String::trim).filter(s -> !s.isBlank()).toList();
            return new CreateRoleRequest(c, t, pc);
        }
    }

    public record UpdateRoleRequest(
        @NotBlank @Size(max = 60) String code,
        @NotBlank @Size(max = 120) String title,
        @NotNull List<@NotBlank String> permissionCodes
    ) {
        public UpdateRoleRequest normalized() {
            final String c = code == null ? null : code.trim().toUpperCase();
            final String t = title == null ? null : title.trim();
            final List<String> pc = permissionCodes == null
                ? List.of()
                : permissionCodes.stream().filter(Objects::nonNull).map(String::trim).filter(s -> !s.isBlank()).toList();
            return new UpdateRoleRequest(c, t, pc);
        }
    }
}
