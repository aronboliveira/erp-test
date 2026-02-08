package com.acme.admin.dto;

import java.util.List;

public final class MeDtos {
    private MeDtos() {}

    public record MeResponse(
        String userId,
        String email,
        String displayName,
        List<String> roleNames,
        List<String> permissionCodes
    ) {}
}
