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
            throw new NotImplementedException();
        }
    }
}
