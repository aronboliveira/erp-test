package com.acme.admin.security;

public interface PermissionGate {
    void assertHas(String permissionCode);
}
