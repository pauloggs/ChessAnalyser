using Services.Helpers;

namespace Services.PlayerMetadata;

/// <summary>
/// Curated list of classical world champions matched against stored (Surname, Forenames) using
/// <see cref="PlayerForenamesMatcher"/> for abbreviation tolerance.
/// </summary>
public static class WorldChampionCatalog
{
    private static readonly (string Surname, string Forenames)[] Champions =
    [
        ("Steinitz", "Wilhelm"),
        ("Lasker", "Emanuel"),
        ("Capablanca", "Jose Raul"),
        ("Alekhine", "Alexander"),
        ("Euwe", "Max"),
        ("Botvinnik", "Mikhail"),
        ("Smyslov", "Vasily"),
        ("Tal", "Mikhail"),
        ("Petrosian", "Tigran"),
        ("Spassky", "Boris"),
        ("Fischer", "Bobby"),
        ("Fischer", "Robert James"),
        ("Karpov", "Anatoly"),
        ("Kasparov", "Garry"),
        ("Kramnik", "Vladimir"),
        ("Anand", "Viswanathan"),
        ("Carlsen", "Magnus"),
        ("Ding", "Liren"),
        ("Gukesh", "Dommaraju"),
    ];

    public static bool IsWorldChampion(string surname, string? forenames)
    {
        if (string.IsNullOrWhiteSpace(surname))
            return false;

        var normalizedForenames = (forenames ?? string.Empty).Trim();
        foreach (var (championSurname, championForenames) in Champions)
        {
            if (!string.Equals(championSurname, surname.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            if (PlayerForenamesMatcher.ForenamesMatch(championForenames, normalizedForenames))
                return true;
        }

        return false;
    }
}
