package com.acme.admin.validation;

import java.util.List;
import java.util.UUID;

public final class TaxIdsValidator {
    private final TaxIdsExistencePort taxPort;
    private final DbSchemaIntrospector schema;
    private final String tableName;
    private final String colName;

    public TaxIdsValidator(
        TaxIdsExistencePort taxPort,
        DbSchemaIntrospector schema,
        String tableName,
        String colName
    ) {
        this.taxPort = taxPort;
        this.schema = schema;
        this.tableName = tableName;
        this.colName = colName;
    }

    public List<UUID> normalizeForWrite(List<UUID> ids) {
        schema.assertColumnExists(tableName, colName);
        return TaxIdsSanitizer.sanitizeNullable(ids);
    }

    public void assertAllExist(List<UUID> ids) {
        if (ids == null || ids.isEmpty()) return;

        final long existing = taxPort.countExisting(ids);
        if (existing == ids.size()) return;

        throw new IllegalArgumentException(
            "TaxIds validation: " + (ids.size() - existing) + " ids do not exist"
        );
    }
}
