package com.acme.admin.provider;

import org.springframework.stereotype.Component;

import java.util.List;

@Component
public final class NoopAccessSnapshotProvider implements AccessSnapshotProvider {
    @Override
    public AccessSnapshot snapshotFor(String userId) {
        return new AccessSnapshot(
            List.of("Admin"),
            List.of("BILLING_CREATE_SESSION", "TAX_READ", "TAX_WRITE")
        );
    }
}
