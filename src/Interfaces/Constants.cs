namespace Interfaces
{
    public static class Constants
    {
        static public List<string> GameTagIdentifiers { get; } =
        [
            "event",
            "site",
            "date",
            "round",
            "white",
            "black",
            "result",
            "whiteelo",
            "blackelo",
            "eco"
        ];

        static public string DefaultEmptyTagValue { get; } = "None";

        public static string GameStartMarker { get; } = "[Event";
    }
}
