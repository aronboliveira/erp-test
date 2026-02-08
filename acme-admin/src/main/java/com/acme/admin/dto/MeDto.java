package com.acme.admin.dto;

import java.util.List;
import java.util.UUID;

public final class MeDto {
    private final UUID id;
    private final String username;
    private final List<String> roleNames;
    private final List<String> permissionCodes;
    private final String createdAt;
    private final String lastLoginAt;

    public MeDto(
        UUID id,
        String username,
        List<String> roleNames,
        List<String> permissionCodes,
        String createdAt,
        String lastLoginAt
    ) {
        this.id = id;
        this.username = username;
        this.roleNames = roleNames;
        this.permissionCodes = permissionCodes;
        this.createdAt = createdAt;
        this.lastLoginAt = lastLoginAt;
    }

    public UUID getId() { return id; }
    public String getUsername() { return username; }
    public List<String> getRoleNames() { return roleNames; }
    public List<String> getPermissionCodes() { return permissionCodes; }
    public String getCreatedAt() { return createdAt; }
    public String getLastLoginAt() { return lastLoginAt; }
}
