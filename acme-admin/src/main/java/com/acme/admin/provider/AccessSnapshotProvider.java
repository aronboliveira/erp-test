package com.acme.admin.provider;

import java.util.List;

public interface AccessSnapshotProvider {
    AccessSnapshot snapshotFor(String userId);

    record AccessSnapshot(
        List<String> roleNames,
        List<String> permissionCodes
    ) {}
}
