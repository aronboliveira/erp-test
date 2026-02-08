package com.acme.admin.service.impl;

import com.acme.admin.dto.MeDto;
import com.acme.admin.service.UserProfileService;
import com.acme.admin.domain.security.AuthUser;
import com.acme.admin.repository.security.AuthUserRepository;

import java.time.OffsetDateTime;
import java.time.Instant;
import java.time.format.DateTimeFormatter;
import java.util.Comparator;
import java.util.List;

import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class DefaultUserProfileService implements UserProfileService {
    private final AuthUserRepository users;
		private static final DateTimeFormatter DT_LOCAL_MIN = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm");

    public DefaultUserProfileService(AuthUserRepository users) {
        this.users = users;
    }

    @Override
    @Transactional(readOnly = true)
    public MeDto getMe(String username) {
        final AuthUser u = users
            .findByUsername(username)
            .orElseThrow(() -> new IllegalArgumentException("User not found"));

        final List<String> roleNames = u.getRoles().stream()
            .map(r -> r.getName())
            .sorted(Comparator.naturalOrder())
            .distinct()
            .toList();

        final List<String> permissionCodes = u.getRoles().stream()
            .flatMap(r -> r.getPermissions().stream())
            .map(p -> p.getCode())
            .sorted(Comparator.naturalOrder())
            .distinct()
            .toList();

        final String createdAt = fmtInstant(u.getCreatedAt());

        return new MeDto(
            u.getId(),
            u.getUsername(),
            roleNames,
            permissionCodes,
            createdAt,
            null
        );
    }

    private static String fmtInstant(Instant instant) {
        if (instant == null) return null;
        return instant.toString();
    }
}
