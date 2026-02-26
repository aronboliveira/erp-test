using Acme.Admin.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Acme.Admin.Api.Data;

public sealed class AcmeDbContext(DbContextOptions<AcmeDbContext> options) : DbContext(options)
{
    public DbSet<TaxEntity> Taxes => Set<TaxEntity>();
    public DbSet<ProductOrServiceCategoryEntity> ProductOrServiceCategories => Set<ProductOrServiceCategoryEntity>();
    public DbSet<ProductOrServiceEntity> ProductsOrServices => Set<ProductOrServiceEntity>();
    public DbSet<ExpenseCategoryEntity> ExpenseCategories => Set<ExpenseCategoryEntity>();
    public DbSet<ExpenseEntity> Expenses => Set<ExpenseEntity>();
    public DbSet<RevenueEntity> Revenues => Set<RevenueEntity>();
    public DbSet<BudgetEntity> Budgets => Set<BudgetEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<PurchaseEntity> Purchases => Set<PurchaseEntity>();
    public DbSet<BillEntity> Bills => Set<BillEntity>();
    public DbSet<HiringEntity> Hirings => Set<HiringEntity>();
    public DbSet<BillingEventEntity> BillingEvents => Set<BillingEventEntity>();

    public DbSet<AuthPermissionEntity> AuthPermissions => Set<AuthPermissionEntity>();
    public DbSet<AuthRoleEntity> AuthRoles => Set<AuthRoleEntity>();
    public DbSet<AuthUserEntity> AuthUsers => Set<AuthUserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTax(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigureFinance(modelBuilder);
        ConfigureSecurity(modelBuilder);
        ConfigureBilling(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchAuditedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        TouchAuditedEntities();
        return base.SaveChanges();
    }

    private void TouchAuditedEntities()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuthPermissionEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuthRoleEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuthUserEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

    private static void ConfigureTax(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<TaxEntity>();
        e.ToTable("taxes");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(180).IsRequired();
        e.Property(x => x.Rate).HasColumnName("rate").HasColumnType("numeric(7,4)").IsRequired();
        e.Property(x => x.Enabled).HasColumnName("enabled").IsRequired();
        ConfigureAudited(e);
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        var category = modelBuilder.Entity<ProductOrServiceCategoryEntity>();
        category.ToTable("product_or_service_categories");
        category.HasKey(x => x.Id);
        category.Property(x => x.Id).HasColumnName("id");
        category.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        category.Property(x => x.Name).HasColumnName("name").HasMaxLength(180).IsRequired();
        category.Property(x => x.Description).HasColumnName("description");
        ConfigureAudited(category);

        var item = modelBuilder.Entity<ProductOrServiceEntity>();
        item.ToTable("products_or_services");
        item.HasKey(x => x.Id);
        item.Property(x => x.Id).HasColumnName("id");
        item.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(20).IsRequired();
        item.Property(x => x.Name).HasColumnName("name").HasMaxLength(180).IsRequired();
        item.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(64);
        item.Property(x => x.Price).HasColumnName("price").HasColumnType("numeric(15,2)").IsRequired();
        item.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        item.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
        item.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        ConfigureAudited(item);

        var expenseCategory = modelBuilder.Entity<ExpenseCategoryEntity>();
        expenseCategory.ToTable("expense_categories");
        expenseCategory.HasKey(x => x.Id);
        expenseCategory.Property(x => x.Id).HasColumnName("id");
        expenseCategory.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        expenseCategory.Property(x => x.Name).HasColumnName("name").HasMaxLength(180).IsRequired();
        expenseCategory.Property(x => x.Subject).HasColumnName("subject").HasConversion<string>().HasMaxLength(32).IsRequired();
        expenseCategory.Property(x => x.Description).HasColumnName("description");
        ConfigureAudited(expenseCategory);
    }

    private static void ConfigureFinance(ModelBuilder modelBuilder)
    {
        var expense = modelBuilder.Entity<ExpenseEntity>();
        expense.ToTable("expenses");
        expense.HasKey(x => x.Id);
        expense.Property(x => x.Id).HasColumnName("id");
        expense.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        expense.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(15,2)").IsRequired();
        expense.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        expense.Property(x => x.Vendor).HasColumnName("vendor").HasMaxLength(180);
        expense.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
        expense.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        ConfigureAudited(expense);

        var revenue = modelBuilder.Entity<RevenueEntity>();
        revenue.ToTable("revenues");
        revenue.HasKey(x => x.Id);
        revenue.Property(x => x.Id).HasColumnName("id");
        revenue.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        revenue.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(15,2)").IsRequired();
        revenue.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        revenue.Property(x => x.SourceRef).HasColumnName("source_ref").HasMaxLength(180);
        ConfigureAudited(revenue);

        var budget = modelBuilder.Entity<BudgetEntity>();
        budget.ToTable("budgets");
        budget.HasKey(x => x.Id);
        budget.Property(x => x.Id).HasColumnName("id");
        budget.Property(x => x.PeriodStart).HasColumnName("period_start").IsRequired();
        budget.Property(x => x.PeriodEnd).HasColumnName("period_end").IsRequired();
        budget.Property(x => x.PlannedAmount).HasColumnName("planned_amount").HasColumnType("numeric(15,2)").IsRequired();
        budget.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        ConfigureAudited(budget);

        var order = modelBuilder.Entity<OrderEntity>();
        order.ToTable("orders");
        order.HasKey(x => x.Id);
        order.Property(x => x.Id).HasColumnName("id");
        order.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        order.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        order.Property(x => x.IssuedAt).HasColumnName("issued_at").IsRequired();
        order.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        order.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(15,2)").IsRequired();
        order.Property(x => x.TaxIds).HasColumnName("tax_ids").HasColumnType("jsonb");
        ConfigureAudited(order);

        var purchase = modelBuilder.Entity<PurchaseEntity>();
        purchase.ToTable("purchases");
        purchase.HasKey(x => x.Id);
        purchase.Property(x => x.Id).HasColumnName("id");
        purchase.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        purchase.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        purchase.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        purchase.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(15,2)").IsRequired();
        purchase.Property(x => x.Vendor).HasColumnName("vendor").HasMaxLength(180);
        purchase.Property(x => x.TaxIds).HasColumnName("tax_ids").HasColumnType("jsonb");
        ConfigureAudited(purchase);

        var bill = modelBuilder.Entity<BillEntity>();
        bill.ToTable("bills");
        bill.HasKey(x => x.Id);
        bill.Property(x => x.Id).HasColumnName("id");
        bill.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        bill.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        bill.Property(x => x.DueAt).HasColumnName("due_at");
        bill.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        bill.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(15,2)").IsRequired();
        bill.Property(x => x.Vendor).HasColumnName("vendor").HasMaxLength(180);
        bill.Property(x => x.Payee).HasColumnName("payee").HasMaxLength(180);
        bill.Property(x => x.TaxIds).HasColumnName("tax_ids").HasColumnType("jsonb");
        ConfigureAudited(bill);

        var hiring = modelBuilder.Entity<HiringEntity>();
        hiring.ToTable("hirings");
        hiring.HasKey(x => x.Id);
        hiring.Property(x => x.Id).HasColumnName("id");
        hiring.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        hiring.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        hiring.Property(x => x.EmployeeName).HasColumnName("employee_name").HasMaxLength(180).IsRequired();
        hiring.Property(x => x.Role).HasColumnName("role").HasMaxLength(120).IsRequired();
        hiring.Property(x => x.StartAt).HasColumnName("start_at").IsRequired();
        hiring.Property(x => x.EndAt).HasColumnName("end_at");
        hiring.Property(x => x.GrossSalary).HasColumnName("gross_salary").HasColumnType("numeric(15,2)").IsRequired();
        hiring.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        hiring.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(15,2)");
        hiring.Property(x => x.CandidateName).HasColumnName("candidate_name").HasMaxLength(180);
        hiring.Property(x => x.TaxIds).HasColumnName("tax_ids").HasColumnType("jsonb");
        ConfigureAudited(hiring);
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        var permission = modelBuilder.Entity<AuthPermissionEntity>();
        permission.ToTable("auth_permissions");
        permission.HasKey(x => x.Id);
        permission.Property(x => x.Id).HasColumnName("id");
        permission.Property(x => x.Code).HasColumnName("code").HasMaxLength(80).IsRequired();
        permission.Property(x => x.Description).HasColumnName("description");
        permission.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        permission.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        var role = modelBuilder.Entity<AuthRoleEntity>();
        role.ToTable("auth_roles");
        role.HasKey(x => x.Id);
        role.Property(x => x.Id).HasColumnName("id");
        role.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();
        role.Property(x => x.Description).HasColumnName("description");
        role.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        role.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        var user = modelBuilder.Entity<AuthUserEntity>();
        user.ToTable("auth_users");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasColumnName("id");
        user.Property(x => x.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        user.Property(x => x.Username).HasColumnName("username").HasMaxLength(60).IsRequired();
        user.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(120);
        user.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        user.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        user.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        user.Property(x => x.Enabled).HasColumnName("enabled").IsRequired();
        user.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        user.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        user.HasMany(x => x.Roles)
            .WithMany(x => x.Users)
            .UsingEntity<Dictionary<string, object>>(
                "auth_user_roles",
                right => right.HasOne<AuthRoleEntity>().WithMany().HasForeignKey("role_id"),
                left => left.HasOne<AuthUserEntity>().WithMany().HasForeignKey("user_id"),
                join =>
                {
                    join.ToTable("auth_user_roles");
                    join.HasKey("user_id", "role_id");
                });

        role.HasMany(x => x.Permissions)
            .WithMany(x => x.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "auth_role_permissions",
                right => right.HasOne<AuthPermissionEntity>().WithMany().HasForeignKey("permission_id"),
                left => left.HasOne<AuthRoleEntity>().WithMany().HasForeignKey("role_id"),
                join =>
                {
                    join.ToTable("auth_role_permissions");
                    join.HasKey("role_id", "permission_id");
                });
    }

    private static void ConfigureBilling(ModelBuilder modelBuilder)
    {
        var billing = modelBuilder.Entity<BillingEventEntity>();
        billing.ToTable("billing_events");
        billing.HasKey(x => x.Id);
        billing.Property(x => x.Id).HasColumnName("id");
        billing.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(32).IsRequired();
        billing.Property(x => x.EventId).HasColumnName("event_id").HasMaxLength(128).IsRequired();
        billing.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(128).IsRequired();
        billing.Property(x => x.ReceivedAt).HasColumnName("received_at").IsRequired();
        billing.Property(x => x.Payload).HasColumnName("payload").IsRequired();
        billing.HasIndex(x => x.EventId).IsUnique();
    }

    private static void ConfigureAudited<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : AuditedEntity
    {
        entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
