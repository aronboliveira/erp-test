package com.acme.admin.service;

import com.acme.admin.domain.TaxEntity;
import com.acme.admin.dto.TaxDtos;
import com.acme.admin.repository.TaxRepository;
import com.acme.admin.time.TemporalFormat;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

@Service
public class TaxService {

    private final TaxRepository repo;

    public TaxService(TaxRepository repo) {
        this.repo = repo;
    }

    @Transactional(readOnly = true)
    public List<TaxDtos.TaxResponse> list() {
        return repo.findAll().stream().map(this::toDto).toList();
    }

    @Transactional
    public TaxDtos.TaxResponse create(TaxDtos.CreateTaxRequest req) {
        if (req == null) throw new IllegalArgumentException("tax: payload required");

        final String code = normalizeCode(req.code());
        if (repo.existsByCode(code)) throw new IllegalArgumentException("tax: code already exists");

        final TaxEntity e = new TaxEntity(
            UUID.randomUUID(),
            code,
            normalizeTitle(req.title()),
            req.rate() == null ? BigDecimal.ZERO : req.rate()
        );

        return toDto(repo.save(e));
    }

    @Transactional
    public TaxDtos.TaxResponse update(UUID id, TaxDtos.UpdateTaxRequest req) {
        if (id == null) throw new IllegalArgumentException("tax: id required");
        if (req == null) throw new IllegalArgumentException("tax: payload required");

        final TaxEntity e = repo.findById(id).orElseThrow(
            () -> new IllegalArgumentException("tax: not found")
        );

        final String code = normalizeCode(req.code());
        if (!e.getCode().equals(code) && repo.existsByCode(code))
            throw new IllegalArgumentException("tax: code already exists");

        applyMutable(e, code, normalizeTitle(req.title()), req.rate());
        return toDto(repo.save(e));
    }

    @Transactional
    public void delete(UUID id) {
        if (id == null) throw new IllegalArgumentException("tax: id required");
        if (!repo.existsById(id)) throw new IllegalArgumentException("tax: not found");
        repo.deleteById(id);
    }

    private TaxDtos.TaxResponse toDto(TaxEntity e) {
        return new TaxDtos.TaxResponse(
            e.getId(),
            e.getCode(),
            e.getName(),
            e.getRate(),
            TemporalFormat.datetimeLocal(e.getCreatedAt())
        );
    }

    private static void applyMutable(TaxEntity e, String code, String name, BigDecimal rate) {
        e.setCode(code);
        e.setName(name);
        e.setRate(rate == null ? BigDecimal.ZERO : rate);
    }

    private static String normalizeCode(String raw) {
        if (raw == null || raw.isBlank()) throw new IllegalArgumentException("tax: code required");
        final String v = raw.trim().toUpperCase();
        if (!v.matches("^[A-Z0-9_\\-]{2,64}$"))
            throw new IllegalArgumentException("tax: code format invalid");
        return v;
    }

    private static String normalizeTitle(String raw) {
        if (raw == null || raw.isBlank()) throw new IllegalArgumentException("tax: title required");
        final String v = raw.trim();
        if (v.length() > 160) throw new IllegalArgumentException("tax: title too long");
        return v;
    }
}
