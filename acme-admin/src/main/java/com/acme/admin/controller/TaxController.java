package com.acme.admin.controller;

import com.acme.admin.dto.TaxDtos;
import com.acme.admin.service.TaxService;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/taxes")
public class TaxController {

    private final TaxService svc;

    public TaxController(TaxService svc) {
        this.svc = svc;
    }

    @GetMapping
    public ResponseEntity<List<TaxDtos.TaxResponse>> list() {
        return ResponseEntity.ok(svc.list());
    }

    @PostMapping
    public ResponseEntity<TaxDtos.TaxResponse> create(@RequestBody TaxDtos.CreateTaxRequest req) {
        return ResponseEntity.status(HttpStatus.CREATED).body(svc.create(req));
    }

    @PutMapping("/{id}")
    public ResponseEntity<TaxDtos.TaxResponse> update(
        @PathVariable("id") UUID id,
        @RequestBody TaxDtos.UpdateTaxRequest req
    ) {
        return ResponseEntity.ok(svc.update(id, req));
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> delete(@PathVariable("id") UUID id) {
        svc.delete(id);
        return ResponseEntity.noContent().build();
    }
}
