package com.acme.admin.validation;

import java.util.*;

public final class TaxIdsSanitizer {
    private TaxIdsSanitizer() {}

    public static final int DEFAULT_MAX = 64;

    public static List<UUID> sanitizeNullable(List<UUID> in) {
        return sanitizeNullable(in, DEFAULT_MAX);
    }

    public static List<UUID> sanitizeNullable(List<UUID> in, int max) {
        if (in == null) return null;
        if (in.isEmpty()) return List.of();

        final int cap = Math.max(0, max);
        final LinkedHashSet<UUID> uniq = new LinkedHashSet<>();

        for (final UUID id : in) {
            if (id == null) continue;
            if (uniq.size() >= cap) break;
            uniq.add(id);
        }

        return List.copyOf(uniq);
    }
}
