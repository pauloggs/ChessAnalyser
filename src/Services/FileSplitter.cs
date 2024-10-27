using Interfaces.DTO;

namespace Services
{
    public interface IFileSplitter
    {
        List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn);
    }

    public class FileSplitter : IFileSplitter
    {
        public List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn)
        {
            var gameStartMarker = "[Event";

            var returnValue = new List<RawGame>();

            var pgnFileName = rawPgn.Name;

            string[] tokens = rawPgn.Contents.Split(new[] { gameStartMarker }, StringSplitOptions.None);
            
            foreach (string token in tokens)
            {
                if (token.Length > 0)
                {
                    returnValue.Add(new RawGame()
                    {
                        ParentPgnFileName = pgnFileName,
                        GameName = "SomeGameName",
                        Contents = (gameStartMarker + token).Trim()
                    });
                }
            }
            
            return returnValue;
        }
    }
}
