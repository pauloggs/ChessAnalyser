using Interfaces.DTO;
using Moq;
using Repositories;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class FideCatalogImportServiceTests
{
    [Fact]
    public async Task ImportAndBackfillAsync_PersistsCatalogAndBackfills()
    {
        var records = new List<FidePlayerRecord>
        {
            new() { FideId = 1, Surname = "Carlsen", Forenames = "Magnus", Title = "GM" }
        };

        var repo = new Mock<IChessRepository>();
        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>())).ReturnsAsync(records);

        var matcher = new Mock<IFidePlayerMatcher>();
        var backfill = new FideMetadataSyncResult { PlayersChecked = 5, PlayersUpdated = 1 };
        var enricher = new Mock<IPlayerFideMetadataEnricher>();
        enricher.Setup(e => e.EnrichAllAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(backfill);

        var sut = new FideCatalogImportService(repo.Object, reader.Object, matcher.Object, enricher.Object);
        var result = await sut.ImportAndBackfillAsync("list.txt");

        Assert.Equal(1, result.CatalogRowsImported);
        Assert.Equal(5, result.Backfill.PlayersChecked);
        repo.Verify(r => r.ReplaceFideCatalogAsync(records, It.IsAny<CancellationToken>()), Times.Once);
        matcher.Verify(m => m.InvalidateCache(), Times.Once);
    }

    [Fact]
    public async Task ImportAndBackfillAsync_DryRun_DoesNotPersistCatalog()
    {
        var records = new List<FidePlayerRecord>
        {
            new() { FideId = 1, Surname = "Carlsen", Forenames = "Magnus" }
        };

        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>())).ReturnsAsync(records);

        var matcher = new Mock<IFidePlayerMatcher>();
        var enricher = new Mock<IPlayerFideMetadataEnricher>();
        enricher.Setup(e => e.EnrichAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FideMetadataSyncResult { DryRun = true });

        var sut = new FideCatalogImportService(
            Mock.Of<IChessRepository>(),
            reader.Object,
            matcher.Object,
            enricher.Object);

        var result = await sut.ImportAndBackfillAsync("list.txt", dryRun: true);

        Assert.True(result.DryRun);
        Assert.Equal(0, result.CatalogRowsImported);
        matcher.Verify(m => m.LoadCatalogSnapshot(records), Times.Once);
    }
}
