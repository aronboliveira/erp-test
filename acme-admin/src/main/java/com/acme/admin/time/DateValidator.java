package com.acme.admin.time;

import java.time.*;
import java.time.format.DateTimeParseException;
import java.time.format.DateTimeFormatter;
import java.time.Instant;

public final class DateValidator {
  private static final DateTimeFormatter DATETIME_LOCAL = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm");

  private DateValidator() {}

  public static boolean isDatetimeLocal(String v) {
      if (v == null || v.isBlank()) return false;
      try {
          LocalDateTime.parse(v.trim(), DATETIME_LOCAL);
          return true;
      } catch (DateTimeParseException e) {
          return false;
      }
  }

  public static LocalDateTime parseDatetimeLocal(String v) {
      if (!isDatetimeLocal(v)) throw new IllegalArgumentException("Invalid datetime-local");
      return LocalDateTime.parse(v.trim(), DATETIME_LOCAL);
  }

  public static void assertRange(LocalDateTime from, LocalDateTime to) {
      if (from == null || to == null) return;
      if (to.isBefore(from)) throw new IllegalArgumentException("Invalid range: to < from");
  }

  public static boolean isLeapYear(int year) {
    return Year.isLeap(year);
  }

  public static Instant parseInstantOrThrow(String iso) {
    try {
      return Instant.parse(iso);
    } catch (DateTimeParseException e) {
      throw new IllegalArgumentException("Invalid ISO-8601 instant: " + iso, e);
    }
  }

  public static LocalDate toUtcDate(Instant at) {
    return at.atZone(ZoneOffset.UTC).toLocalDate();
  }

  public static boolean isFuture(Instant at, Clock clock, Duration skew) {
    final Instant now = clock.instant();
    return at.isAfter(now.plus(skew));
  }

  public static boolean isTooOld(Instant at, Clock clock, Duration maxAge) {
    final Instant now = clock.instant();
    return at.isBefore(now.minus(maxAge));
  }

  public static Instant requireNotFuture(Instant v, Instant now, String field) {
    if (v == null) return null;
    if (now == null) now = Instant.now();
    if (v.isAfter(now)) throw new IllegalArgumentException(field + " cannot be in the future");
    return v;
  }

  public static void requireOrder(Instant a, Instant b, String aName, String bName) {
    if (a == null || b == null) return;
    if (b.isBefore(a)) throw new IllegalArgumentException(bName + " cannot be earlier than " + aName);
  }
}
