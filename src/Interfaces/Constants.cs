namespace Interfaces
{
    public static class Constants
    {
        static public List<string> GameTagIdentifiers { get; } =
        [
            "Event",
            "Site",
            "Date",
            "Round",
            "White",
            "Black",
            "Result",
            "WhiteElo",
            "BlackElo",
            "ECO"
        ];

        static public string DefaultEmptyTagValue { get; } = "None";
    }
}
