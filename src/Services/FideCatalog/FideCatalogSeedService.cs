using Interfaces.DTO;
using Microsoft.Extensions.Options;
using Repositories;
using Services.Helpers;

namespace Services.FideCatalog;

/// <inheritdoc />
public sealed class FideCatalogSeedService(
    IChessRepository repository,
    IFideRatingListReader reader,
    IOptions<FideCatalogSeedOptions> options) : IFideCatalogSeedService
{
    private const int BulkBatchSize = 5000;

    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IFideRatingListReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    private readonly FideCatalogSeedOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public async Task<FideCatalogSeedResult> SeedAsync(
        string? listPath = null,
        bool force = false,
        IProgress<MaintenanceProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(listPath);
        if (!File.Exists(path))
        {
            return new FideCatalogSeedResult
            {
                Skipped = true,
                SkipReason = $"FIDE list file not found: {path}",
                SourcePath = path
            };
        }

        var existingCount = await _repository.GetFideCatalogCountAsync(cancellationToken);
        if (existingCount > 0 && !force)
        {
            return new FideCatalogSeedResult
            {
                Skipped = true,
                SkipReason = $"Ref.FidePlayer already has {existingCount} rows. Pass force=true to reload.",
                SourcePath = path
            };
        }

        if (force && existingCount > 0)
        {
            await _repository.ClearAllFidePlayerLinksAsync(cancellationToken);
            await _repository.TruncateFideCatalogAsync(cancellationToken);
        }

        var inserted = 0;
        var skipped = 0;
        var batch = new List<FidePlayer>(BulkBatchSize);

        Report(progress, "Running", 0, null, 0);

        foreach (var line in File.ReadLines(path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_reader.IsHeaderLine(line) || !_reader.IsDataLine(line))
            {
                if (_reader.IsDataLine(line) == false && !_reader.IsHeaderLine(line) && !string.IsNullOrWhiteSpace(line))
                    skipped++;
                continue;
            }

            var row = _reader.ParseDataLine(line);
            if (row is null)
            {
                skipped++;
                continue;
            }

            batch.Add(new FidePlayer
            {
                Id = row.Id,
                Surname = row.Surname,
                Forenames = row.Forenames,
                Federation = row.Federation,
                Sex = row.Sex,
                FideTitle = row.FideTitle,
                BirthYear = row.BirthYear
            });

            if (batch.Count >= BulkBatchSize)
            {
                await _repository.BulkInsertFidePlayersAsync(batch, cancellationToken);
                inserted += batch.Count;
                batch.Clear();
                Report(progress, "Running", null, $"Inserted {inserted:N0} rows…", inserted);
            }
        }

        if (batch.Count > 0)
        {
            await _repository.BulkInsertFidePlayersAsync(batch, cancellationToken);
            inserted += batch.Count;
        }

        Report(progress, "Completed", 100, $"Inserted {inserted:N0} rows.", inserted);

        return new FideCatalogSeedResult
        {
            RowsInserted = inserted,
            RowsSkipped = skipped,
            SourcePath = path
        };
    }

    private string ResolvePath(string? listPath)
    {
        var configured = string.IsNullOrWhiteSpace(listPath) ? _options.ListPath : listPath.Trim();
        return RepoRootLocator.ResolveRelativePath(configured);
    }

    private static void Report(
        IProgress<MaintenanceProgress>? progress,
        string status,
        int? percent,
        string? message,
        int rowsProcessed)
    {
        progress?.Report(new MaintenanceProgress
        {
            Operation = "SeedFideCatalog",
            Status = status,
            PercentComplete = percent,
            Message = message,
            RowsProcessed = rowsProcessed
        });
    }
}
