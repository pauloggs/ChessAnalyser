namespace Repositories
{
    public interface IGameRepository
    {
        void InsertGame(string gameName);
    }

    public class GameRepository : IGameRepository
    {
        public void InsertGame(string gameName)
        {
            throw new NotImplementedException();
        }
    }
}
