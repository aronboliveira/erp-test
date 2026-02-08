package com.acme.admin.service;

import org.springframework.stereotype.Component;

import java.util.*;

@Component
public class TaxIdNormalizer {
  public List<UUID> normalize(List<UUID> ids) {
    if (ids == null) return null;
    final LinkedHashSet<UUID> unique = new LinkedHashSet<>();
    for (UUID id : ids) { if (id != null) unique.add(id); }
    return unique.isEmpty() ? List.of() : List.copyOf(unique);
  }
}
