package com.acme.admin.time;

import java.time.DayOfWeek;
import java.time.LocalDate;
import java.time.Instant;

public final class UTCDateMapper implements DateMapper {
  @Override
  public boolean isBusinessDay(LocalDate d) {
    final DayOfWeek wd = d.getDayOfWeek();
    return wd != DayOfWeek.SATURDAY && wd != DayOfWeek.SUNDAY;
  }
  @Override
  public Instant now() {
    return Instant.now();
  }
}
