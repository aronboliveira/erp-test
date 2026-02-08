package com.acme.admin.security;

import org.springframework.stereotype.Component;

@Component
public final class NoopAuthContext implements AuthContext {
    @Override public String userId() { return "00000000-0000-0000-0000-000000000001"; }
    @Override public String email() { return "admin@example.com"; }
    @Override public String displayName() { return "Admin"; }
}
