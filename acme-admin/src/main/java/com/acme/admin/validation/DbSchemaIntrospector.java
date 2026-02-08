package com.acme.admin.validation;

import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Component;

@Component
public final class DbSchemaIntrospector {
    private final JdbcTemplate jdbc;

    public DbSchemaIntrospector(JdbcTemplate jdbc) {
        this.jdbc = jdbc;
    }

    public boolean hasColumn(String table, String column) {
        final String sql =
            "select count(1) from information_schema.columns " +
            "where table_name = ? and column_name = ?";
        final Integer c = jdbc.queryForObject(sql, Integer.class, table, column);
        return c != null && c > 0;
    }

    public void assertColumnExists(String table, String column) {
        if (hasColumn(table, column)) return;
        throw new IllegalStateException(
            "DDL guard: missing column " + table + "." + column +
            " (apply migrations before writing taxIds)"
        );
    }
}
