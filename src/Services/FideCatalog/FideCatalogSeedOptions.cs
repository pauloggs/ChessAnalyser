namespace Services.FideCatalog;

public sealed class FideCatalogSeedOptions
{
    public const string SectionName = "FideCatalog";

    public string ListPath { get; set; } = "data/fide/players_list_foa.txt";
}
