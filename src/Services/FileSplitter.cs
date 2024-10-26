using Interfaces.DTO;

namespace Services
{
    public interface IFileSplitter
    {
        List<RawGame> GetRawGamesFromPgnFile(string pgnFile);
    }

    public class FileSplitter : IFileSplitter
    {
        public List<RawGame> GetRawGamesFromPgnFile(string pgnFile)
        {
            var returnValue = new List<RawGame>();

            string[] tokens = pgnFile.Split(new[] { "[Event" }, StringSplitOptions.None);
            
            foreach (string token in tokens)
            {
                if (token.Length > 0)
                {
                    returnValue.Add(new RawGame()
                    {
                        Contents = token
                    });
                }
            }
            
            return returnValue;
        }
    }
}
