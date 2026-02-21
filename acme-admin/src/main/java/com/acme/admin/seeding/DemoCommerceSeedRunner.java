package com.acme.admin.seeding;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Component;
import org.springframework.transaction.PlatformTransactionManager;
import org.springframework.transaction.support.TransactionTemplate;

import java.math.BigDecimal;
import java.sql.Timestamp;
import java.time.Duration;
import java.time.Instant;
import java.time.LocalDate;
import java.time.ZoneOffset;
import java.util.List;
import java.util.Locale;
import java.util.Objects;
import java.util.UUID;
import java.util.concurrent.ThreadLocalRandom;

@Component
@ConditionalOnProperty(prefix = "acme.seed.demo", name = "enabled", havingValue = "true")
public final class DemoCommerceSeedRunner implements CommandLineRunner {

    private static final int TARGET_PRODUCTS = 24;
    private static final int TARGET_REVENUES = 180;
    private static final int TARGET_EXPENSES = 220;

    private final JdbcTemplate jdbc;
    private final TransactionTemplate tx;

    public DemoCommerceSeedRunner(JdbcTemplate jdbc, PlatformTransactionManager tm) {
        this.jdbc = jdbc;
        this.tx = new TransactionTemplate(tm);
    }

    @Override
    public void run(String... args) {
        tx.executeWithoutResult((status) -> {
            try {
                if (!tableExists("products_or_services")) return;
                seedProductsIfNeeded();
                seedBudgetsIfNeeded();
                seedFinanceEventsIfNeeded();
            } catch (Exception e) {
                System.err.println("DemoCommerceSeedRunner failed: " + e.getMessage());
                status.setRollbackOnly();
            }
        });
    }

    private boolean tableExists(String table) {
        try (var c = Objects.requireNonNull(jdbc.getDataSource()).getConnection()) {
            final var md = c.getMetaData();
            for (final String name : List.of(table, table.toUpperCase(Locale.ROOT), table.toLowerCase(Locale.ROOT))) {
                try (var rs = md.getTables(null, null, name, new String[]{"TABLE"})) {
                    if (rs.next()) return true;
                }
            }
            return false;
        } catch (Exception e) {
            return false;
        }
    }

    private void seedProductsIfNeeded() {
        final long existing = count("select count(*) from products_or_services");
        if (existing >= TARGET_PRODUCTS) return;

        final UUID catProducts = idByCode("product_or_service_categories", "PRODUCTS");
        final UUID catServices = idByCode("product_or_service_categories", "SERVICES");
        final UUID catSubs = idByCode("product_or_service_categories", "SUBSCRIPTIONS");

        final List<ProductSeed> items = List.of(
            new ProductSeed("PRD-CHAIR-01", "Ergo Chair", "PRODUCT", bd("799.00"), catProducts),
            new ProductSeed("PRD-MON-27", "Monitor 27\"", "PRODUCT", bd("1399.00"), catProducts),
            new ProductSeed("PRD-KBD-01", "Mechanical Keyboard", "PRODUCT", bd("349.00"), catProducts),
            new ProductSeed("PRD-MOUSE-01", "Mouse Pro", "PRODUCT", bd("159.00"), catProducts),
            new ProductSeed("PRD-DOCK-01", "USB-C Dock", "PRODUCT", bd("299.00"), catProducts),
            new ProductSeed("PRD-CAM-01", "Webcam HD", "PRODUCT", bd("199.00"), catProducts),
            new ProductSeed("PRD-MIC-01", "Microphone", "PRODUCT", bd("259.00"), catProducts),
            new ProductSeed("PRD-SSD-01", "External SSD 1TB", "PRODUCT", bd("499.00"), catProducts),
            new ProductSeed("SRV-CONS-01", "Consulting Hour", "SERVICE", bd("220.00"), catServices),
            new ProductSeed("SRV-IMPL-01", "Implementation Package", "SERVICE", bd("1800.00"), catServices),
            new ProductSeed("SRV-MAINT-01", "Maintenance (Monthly)", "SERVICE", bd("890.00"), catServices),
            new ProductSeed("SRV-SUP-01", "Support (Monthly)", "SERVICE", bd("590.00"), catServices),
            new ProductSeed("SRV-AUD-01", "Audit & Review", "SERVICE", bd("2400.00"), catServices),
            new ProductSeed("SRV-TRAIN-01", "Training Session", "SERVICE", bd("1200.00"), catServices),
            new ProductSeed("SRV-MIG-01", "Migration Service", "SERVICE", bd("3200.00"), catServices),
            new ProductSeed("SRV-API-01", "API Integration", "SERVICE", bd("2800.00"), catServices),
            new ProductSeed("SRV-CLOUD-01", "Cloud Setup", "SERVICE", bd("3500.00"), catServices),
            new ProductSeed("SRV-SEC-01", "Security Hardening", "SERVICE", bd("3900.00"), catServices),
            new ProductSeed("SRV-OPS-01", "Ops Retainer", "SERVICE", bd("1600.00"), catServices),
            new ProductSeed("SRV-UX-01", "UX Workshop", "SERVICE", bd("1500.00"), catServices),
            new ProductSeed("SRV-ANL-01", "Analytics Setup", "SERVICE", bd("1400.00"), catServices),
            new ProductSeed("SRV-SEO-01", "SEO Audit", "SERVICE", bd("900.00"), catServices),
            new ProductSeed("SUB-TIER-01", "Subscription Tier 1", "SERVICE", bd("49.00"), catSubs),
            new ProductSeed("SUB-TIER-02", "Subscription Tier 2", "SERVICE", bd("99.00"), catSubs)
        );

        for (ProductSeed item : items) {
            if (item.categoryId() == null) continue;
            jdbc.update("""
                insert into products_or_services
                  (id, category_id, kind, code, sku, name, unit_price, price, currency, active)
                values
                  (?, ?, ?, ?, ?, ?, ?, ?, 'BRL', true)
                on conflict (code) do nothing
            """,
                UUID.randomUUID(),
                item.categoryId(),
                item.kind(),
                item.code(),
                item.code(),
                item.name(),
                item.price(),
                item.price()
            );
        }
    }

    private void seedBudgetsIfNeeded() {
        final long existing = count("select count(*) from budgets");
        if (existing >= 4) return;

        final LocalDate now = LocalDate.now(ZoneOffset.UTC);
        final int year = now.getYear();

        insertBudget("BUD-Q1-" + year, "Q1 Budget", LocalDate.of(year, 1, 1), LocalDate.of(year, 3, 31), bd("3500000.00"), bd("1900000.00"));
        insertBudget("BUD-Q2-" + year, "Q2 Budget", LocalDate.of(year, 4, 1), LocalDate.of(year, 6, 30), bd("4200000.00"), bd("2100000.00"));
        insertBudget("BUD-Q3-" + year, "Q3 Budget", LocalDate.of(year, 7, 1), LocalDate.of(year, 9, 30), bd("4800000.00"), bd("2450000.00"));

        final LocalDate monthStart = now.withDayOfMonth(1);
        final LocalDate monthEnd = monthStart.plusMonths(1).minusDays(1);
        insertBudget("BUD-MONTH-" + year + "-" + String.format(Locale.ROOT, "%02d", now.getMonthValue()),
            "Current Month", monthStart, monthEnd, bd("1600000.00"), bd("900000.00"));
    }

    private void seedFinanceEventsIfNeeded() {
        final long revenues = count("select count(*) from revenues");
        final long expenses = count("select count(*) from expenses");

        if (revenues < TARGET_REVENUES) {
            final int missing = (int) (TARGET_REVENUES - revenues);
            for (int i = 0; i < missing; i++) {
                final Instant at = Instant.now().minus(Duration.ofHours(6L * i));
                final BigDecimal amount = randomMoney(65_000L, 42_000L, 1_200L, 380_000L);
                final String source = (i % 9 == 0) ? "invoice" : "order";
                final String sourceRef = (source.equals("invoice") ? "INV-" : "ORD-") + String.format(Locale.ROOT, "%05d", 1000 + i);
                final String code = String.format(Locale.ROOT, "REV-%06d", revenues + i + 1);

                jdbc.update("""
                    insert into revenues (id, code, occurred_at, description, source, source_ref, total, amount, currency)
                    values (?, ?, ?, ?, ?, ?, ?, ?, 'BRL')
                    on conflict (code) do nothing
                """,
                    UUID.randomUUID(),
                    code,
                    Timestamp.from(at),
                    (i % 17 == 0 ? "Promo spike" : null),
                    source,
                    sourceRef,
                    amount,
                    amount
                );
            }
        }

        final List<UUID> expenseCategories = listIds("select id from expense_categories order by code");
        if (expenses < TARGET_EXPENSES && !expenseCategories.isEmpty()) {
            final int missing = (int) (TARGET_EXPENSES - expenses);
            for (int i = 0; i < missing; i++) {
                final Instant at = Instant.now().minus(Duration.ofHours(5L * i));
                final BigDecimal amount = randomMoney(42_000L, 36_000L, 800L, 320_000L);
                final UUID categoryId = expenseCategories.get(i % expenseCategories.size());
                final String code = String.format(Locale.ROOT, "EXP-%06d", expenses + i + 1);
                final String subject = switch (i % 4) {
                    case 0 -> "OPERATIONAL";
                    case 1 -> "ADMINISTRATIVE";
                    case 2 -> "FINANCIAL";
                    default -> "OTHER";
                };

                jdbc.update("""
                    insert into expenses (id, code, occurred_at, description, subject, category_id, total, amount, currency, vendor)
                    values (?, ?, ?, ?, ?, ?, ?, ?, 'BRL', ?)
                    on conflict (code) do nothing
                """,
                    UUID.randomUUID(),
                    code,
                    Timestamp.from(at),
                    (i % 19 == 0 ? "Monthly renewal" : null),
                    subject,
                    categoryId,
                    amount,
                    amount,
                    "Vendor " + ((i % 12) + 1)
                );
            }
        }
    }

    private void insertBudget(
        String code,
        String name,
        LocalDate periodStart,
        LocalDate periodEnd,
        BigDecimal totalAmount,
        BigDecimal plannedAmount
    ) {
        jdbc.update("""
            insert into budgets
              (id, code, name, description, period_start, period_end, total_amount, planned_amount, currency)
            values
              (?, ?, ?, ?, ?, ?, ?, ?, 'BRL')
            on conflict (code) do nothing
        """,
            UUID.randomUUID(),
            code,
            name,
            "Auto-seeded demo budget",
            periodStart,
            periodEnd,
            totalAmount,
            plannedAmount
        );
    }

    private UUID idByCode(String table, String code) {
        try {
            return jdbc.queryForObject(
                "select id from " + table + " where code = ? limit 1",
                (rs, i) -> UUID.fromString(rs.getString("id")),
                code
            );
        } catch (Exception e) {
            return null;
        }
    }

    private List<UUID> listIds(String sql) {
        return jdbc.query(sql, (rs, i) -> UUID.fromString(rs.getString(1)));
    }

    private long count(String sql) {
        final Long v = jdbc.queryForObject(sql, Long.class);
        return v == null ? 0L : v;
    }

    private static BigDecimal randomMoney(long meanCents, long stdDevCents, long minCents, long maxCents) {
        long cents = Math.round(ThreadLocalRandom.current().nextGaussian() * stdDevCents + meanCents);
        cents = Math.max(minCents, Math.min(maxCents, cents));
        return BigDecimal.valueOf(cents, 2);
    }

    private static BigDecimal bd(String value) {
        return new BigDecimal(value);
    }

    private record ProductSeed(
        String code,
        String name,
        String kind,
        BigDecimal price,
        UUID categoryId
    ) {}
}
