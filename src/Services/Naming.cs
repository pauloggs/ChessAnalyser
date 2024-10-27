namespace Services
{
    public interface INaming
    {
        string GetGameName(Dictionary<string, string> gameTags);
    }

    public class Naming : INaming
    {
        public string GetGameName(Dictionary<string, string> gameTags)
        {
            // <White>_<Black>_<Date>_<Event>_<Location>_<Round>
            return
                "White:" + gameTags["White"].Replace(" ", "-") + "|" +
                "Black:" + gameTags["Black"].Replace(" ", "-") + "|" +
                "Date:" + gameTags["Date"].Replace(" ", "-") + "|" +
                "Event:" + gameTags["Event"].Replace(" ", "-") + "|" +
                "Site:" + gameTags["Site"].Replace(" ", "-") + "|" +
                "Round:" + gameTags["Round"].Replace(" ", "-");
        }
    }
}
