package com.acme.admin.security;

public interface AuthContext {
    String userId();
    String email();
    String displayName();
}
