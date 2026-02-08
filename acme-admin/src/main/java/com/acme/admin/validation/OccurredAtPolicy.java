package com.acme.admin.validation;

import java.time.Duration;

public record OccurredAtPolicy(
  boolean businessDaysOnly,
  Duration maxAge,
  Duration futureSkew
) {}
