package com.acme.admin.security.config;

import jakarta.servlet.*;
import jakarta.servlet.http.*;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;
import org.springframework.security.core.*;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.context.SecurityContextHolder;

import java.io.IOException;
import java.util.List;

@Component
@Profile({"dev", "test"})
public final class MockUserAuthFilter extends OncePerRequestFilter {

    @Override
    protected void doFilterInternal(
        HttpServletRequest req,
        HttpServletResponse res,
        FilterChain chain
    ) throws ServletException, IOException {

        final String user = req.getHeader("X-Mock-User");
        if (user == null || user.isBlank()) { chain.doFilter(req, res); return; }

        final String perms = req.getHeader("X-Mock-Perms");
        final List<GrantedAuthority> auths =
            perms == null || perms.isBlank()
                ? List.of()
                : List.of(perms.split("\\s*,\\s*")).stream()
                    .filter(s -> s != null && !s.isBlank())
                    .map(String::trim)
                    .<GrantedAuthority>map(org.springframework.security.core.authority.SimpleGrantedAuthority::new)
                    .toList();

        final Authentication a = new UsernamePasswordAuthenticationToken(user.trim(), "N/A", auths);
        SecurityContextHolder.getContext().setAuthentication(a);

        chain.doFilter(req, res);
    }
}
