package com.acme.admin.service;

import com.acme.admin.dto.MeDtos;
import com.acme.admin.provider.AccessSnapshotProvider;
import com.acme.admin.security.AuthContext;
import org.springframework.stereotype.Service;

@Service
public class MeService {

    private final AuthContext auth;
    private final AccessSnapshotProvider access;

    public MeService(AuthContext auth, AccessSnapshotProvider access) {
        this.auth = auth;
        this.access = access;
    }

    public MeDtos.MeResponse me() {
        final var a = access.snapshotFor(auth.userId());
        return new MeDtos.MeResponse(
            auth.userId(),
            auth.email(),
            auth.displayName(),
            a.roleNames(),
            a.permissionCodes()
        );
    }
}
