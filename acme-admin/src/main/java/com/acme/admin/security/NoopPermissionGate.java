package com.acme.admin.security;

import org.springframework.stereotype.Component;

@Component
public final class NoopPermissionGate implements PermissionGate {
    @Override
    public void assertHas(String permissionCode) {}
}
