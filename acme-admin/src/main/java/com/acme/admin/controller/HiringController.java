package com.acme.admin.controller;

import com.acme.admin.domain.HiringEntity;
import com.acme.admin.dto.HiringCreateRequest;
import com.acme.admin.service.HiringService;
import com.acme.admin.time.DateMapper;
import com.acme.admin.time.UTCDateMapper;
import com.acme.admin.validation.*;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.time.*;
import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/hr/hirings")
public class HiringController {
  private final HiringService service;
  private final IdsExistenceValidator<UUID> taxIdsValidator;

  private final Clock clock = Clock.systemUTC();
  private final DateMapper dateMapper = new UTCDateMapper();

  private final OccurredAtPolicy occurredAtPolicy =
    new OccurredAtPolicy(true, Duration.ofDays(3650), Duration.ofMinutes(5));

  private final TaxIdsPolicy taxPolicy =
    new TaxIdsPolicy(64, true);

  public HiringController(
    HiringService service,
    IdsExistenceValidator<UUID> taxIdsValidator
  ) {
    this.service = service;
    this.taxIdsValidator = taxIdsValidator;
  }

  @GetMapping
  public List<HiringEntity> list() {
    return service.list();
  }

  @PostMapping
  public HiringEntity create(@Valid @RequestBody HiringCreateRequest req) {
    final HiringEntity e = new HiringEntity();
    e.setCode(req.code());
    e.setOccurredAt(req.occurredAt());
    e.setCurrency(req.currency());
    e.setTotal(req.total());
    e.setCandidateName(req.candidateName());
    e.setTaxIds(req.taxIds());

    final ValidationResult r = e.validateForUpsert(clock, dateMapper, occurredAtPolicy, taxPolicy);
    taxIdsValidator.validate("taxIds", req.taxIds(), r);

    if (r.hasErrors()) throw new ApiValidationException(r.toList());

    return service.create(req);
  }
}
