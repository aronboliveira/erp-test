using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Acme.Admin.Api.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Acme.Admin.Api.Data;

public sealed class SchemaMigrationHostedService(
    IServiceProvider serviceProvider,
    IOptions<SchemaMigrationOptions> options,
    ILogger<SchemaMigrationHostedService> logger) : IHostedService
{
    private const int SqlTimeoutSeconds = 180;
    private const string HistoryTable = "dotnet_schema_migrations";
    private const string ResourcePrefix = "Acme.Admin.Api.Database.Migrations.";

    // Existing Java-managed databases already contain these baseline scripts.
    private static readonly HashSet<string> BaselineOnExistingSchema = new(StringComparer.OrdinalIgnoreCase)
    {
        "V001__initial_schema.sql",
        "V002__financial_transactions.sql",
        "V003__billing_events.sql",
        "V004__seed_data.sql"
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Schema migration runner is disabled.");
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AcmeDbContext>();

        await using var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        await EnsureHistoryTableAsync(connection, cancellationToken);
        await EnsurePgCryptoAsync(connection, cancellationToken);

        var scripts = LoadEmbeddedScripts();
        if (scripts.Count == 0)
        {
            logger.LogWarning("No embedded SQL migration scripts were found.");
            return;
        }

        var history = await LoadHistoryAsync(connection, cancellationToken);
        var hasLegacySchema = await HasLegacySchemaAsync(connection, cancellationToken);
        await BaselineExistingSchemaIfNeededAsync(connection, scripts, history, hasLegacySchema, cancellationToken);

        foreach (var script in scripts.Where(x => x.Kind == MigrationScriptKind.Versioned))
        {
            if (history.ContainsKey(script.FileName))
            {
                continue;
            }

            await ApplyScriptAsync(connection, script, cancellationToken);
            history[script.FileName] = script.Checksum;
        }

        foreach (var script in scripts.Where(x => x.Kind == MigrationScriptKind.Repeatable))
        {
            if (history.TryGetValue(script.FileName, out var checksum) &&
                string.Equals(checksum, script.Checksum, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await ApplyScriptAsync(connection, script, cancellationToken);
            history[script.FileName] = script.Checksum;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task BaselineExistingSchemaIfNeededAsync(
        NpgsqlConnection connection,
        IReadOnlyList<MigrationScript> scripts,
        Dictionary<string, string> history,
        bool hasLegacySchema,
        CancellationToken cancellationToken)
    {
        if (!options.Value.BaselineExistingSchema || !hasLegacySchema || history.Count != 0)
        {
            return;
        }

        var baselineScripts = scripts
            .Where(x => x.Kind == MigrationScriptKind.Versioned && BaselineOnExistingSchema.Contains(x.FileName))
            .OrderBy(x => x.Version)
            .ToList();

        if (baselineScripts.Count == 0)
        {
            return;
        }

        foreach (var script in baselineScripts)
        {
            await UpsertHistoryAsync(connection, script, cancellationToken);
            history[script.FileName] = script.Checksum;
        }

        logger.LogInformation(
            "Detected existing schema and baselined {Count} versioned scripts in {HistoryTable}.",
            baselineScripts.Count,
            HistoryTable);
    }

    private async Task ApplyScriptAsync(NpgsqlConnection connection, MigrationScript script, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying migration script {ScriptName}", script.FileName);

        await ExecuteNonQueryAsync(connection, script.Sql, cancellationToken);
        await UpsertHistoryAsync(connection, script, cancellationToken);
    }

    private static async Task EnsureHistoryTableAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await ExecuteNonQueryAsync(
            connection,
            $"""
             CREATE TABLE IF NOT EXISTS {HistoryTable} (
                 script_name VARCHAR(200) PRIMARY KEY,
                 checksum VARCHAR(64) NOT NULL,
                 kind VARCHAR(20) NOT NULL,
                 applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
             );
             """,
            cancellationToken);
    }

    private static async Task EnsurePgCryptoAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await ExecuteNonQueryAsync(connection, "CREATE EXTENSION IF NOT EXISTS pgcrypto;", cancellationToken);
    }

    private static async Task<Dictionary<string, string>> LoadHistoryAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        var history = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = SqlTimeoutSeconds;
        command.CommandText = $"SELECT script_name, checksum FROM {HistoryTable};";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var script = reader.GetString(0);
            var checksum = reader.GetString(1);
            history[script] = checksum;
        }

        return history;
    }

    private static async Task<bool> HasLegacySchemaAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandTimeout = SqlTimeoutSeconds;
        command.CommandText =
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name IN ('auth_users', 'orders', 'billing_events')
            );
            """;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    private static async Task UpsertHistoryAsync(NpgsqlConnection connection, MigrationScript script, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandTimeout = SqlTimeoutSeconds;
        command.CommandText =
            $"""
             INSERT INTO {HistoryTable} (script_name, checksum, kind, applied_at)
             VALUES (@name, @checksum, @kind, NOW())
             ON CONFLICT (script_name) DO UPDATE
             SET checksum = EXCLUDED.checksum,
                 kind = EXCLUDED.kind,
                 applied_at = NOW();
             """;
        command.Parameters.AddWithValue("name", script.FileName);
        command.Parameters.AddWithValue("checksum", script.Checksum);
        command.Parameters.AddWithValue("kind", script.Kind.ToString());

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandTimeout = SqlTimeoutSeconds;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static IReadOnlyList<MigrationScript> LoadEmbeddedScripts()
    {
        var assembly = typeof(Program).Assembly;
        var scripts = new List<MigrationScript>();

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal) ||
                !resourceName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fileName = resourceName[ResourcePrefix.Length..];
            var kind = ResolveKind(fileName);
            if (kind == MigrationScriptKind.Unknown)
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var sql = reader.ReadToEnd();
            var checksum = ComputeChecksum(sql);
            var version = kind == MigrationScriptKind.Versioned ? ParseVersion(fileName) : int.MaxValue;

            scripts.Add(new MigrationScript(fileName, kind, version, checksum, sql));
        }

        return scripts
            .OrderBy(x => x.Kind == MigrationScriptKind.Versioned ? 0 : 1)
            .ThenBy(x => x.Version)
            .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static MigrationScriptKind ResolveKind(string fileName)
    {
        if (fileName.StartsWith("V", StringComparison.OrdinalIgnoreCase) &&
            fileName.Contains("__", StringComparison.Ordinal))
        {
            return MigrationScriptKind.Versioned;
        }

        if (fileName.StartsWith("R__", StringComparison.OrdinalIgnoreCase))
        {
            return MigrationScriptKind.Repeatable;
        }

        return MigrationScriptKind.Unknown;
    }

    private static int ParseVersion(string fileName)
    {
        var separator = fileName.IndexOf("__", StringComparison.Ordinal);
        if (separator < 2)
        {
            throw new InvalidOperationException($"Invalid versioned migration name: {fileName}");
        }

        var span = fileName.AsSpan(1, separator - 1);
        if (!int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version))
        {
            throw new InvalidOperationException($"Invalid version prefix in migration: {fileName}");
        }

        return version;
    }

    private static string ComputeChecksum(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private enum MigrationScriptKind
    {
        Unknown = 0,
        Versioned = 1,
        Repeatable = 2
    }

    private sealed record MigrationScript(
        string FileName,
        MigrationScriptKind Kind,
        int Version,
        string Checksum,
        string Sql);
}
