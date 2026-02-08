package com.acme.admin.time;

import java.time.Instant;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.ZoneOffset;
import java.time.format.DateTimeFormatter;

public interface DateMapper {
  DateTimeFormatter DATETIME_LOCAL = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm");
  
  Instant now();

  boolean isBusinessDay(LocalDate date);

  static Instant toUtcInstant(LocalDateTime ldt) {
      return ldt == null ? null : ldt.toInstant(ZoneOffset.UTC);
  }

  static String toDatetimeLocal(Instant instant) {
      if (instant == null) return null;
      final var ldt = LocalDateTime.ofInstant(instant, ZoneOffset.UTC);
      return DATETIME_LOCAL.format(ldt);
  }
}
