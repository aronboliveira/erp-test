package com.acme.admin.time;

import java.time.Instant;
import java.time.OffsetDateTime;
import java.time.format.DateTimeFormatter;

public final class TemporalFormat {
    private TemporalFormat() {}

    private static final DateTimeFormatter DT_LOCAL_MIN =
        DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm");

    public static String datetimeLocal(Instant instant) {
        return DateMapper.toDatetimeLocal(instant);
    }
}
