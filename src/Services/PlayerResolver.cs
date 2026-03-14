using Interfaces.DTO;
using Repositories;
using Services.Helpers;

namespace Services
{
    /// <summary>
    /// Resolves White/Black tag values to Player IDs: loads known players, parses names, inserts new players when needed.
    /// Call LoadKnownPlayersAsync once before processing games; then ResolveGamePlayersAsync for each game.
    /// </summary>
    public interface IPlayerResolver
    {
        /// <summary>Load all players from the database into the in-memory cache. Call once before processing a run.</summary>
        Task LoadKnownPlayersAsync();

        /// <summary>Resolve White and Black from game.Tags, set game.WhitePlayerId and game.BlackPlayerId (look up or insert).</summary>
        Task ResolveGamePlayersAsync(Game game);
    }

    public class PlayerResolver(IChessRepository chessRepository) : IPlayerResolver
    {
        private readonly IChessRepository _chessRepository = chessRepository;
        private readonly Dictionary<(string Surname, string Forenames), int> _cache = new();

        public async Task LoadKnownPlayersAsync()
        {
            _cache.Clear();
            var players = await _chessRepository.GetPlayers();
            foreach (var p in players)
            {
                var key = (p.Surname ?? string.Empty, p.Forenames ?? string.Empty);
                if (!_cache.ContainsKey(key))
                    _cache[key] = p.Id;
            }
        }

        public async Task ResolveGamePlayersAsync(Game game)
        {
            if (game.Tags == null)
                return;

            if (game.Tags.TryGetValue("white", out var whiteValue))
            {
                PlayerNameParser.Parse(whiteValue, out var surname, out var forenames);
                if (!string.IsNullOrWhiteSpace(surname))
                    game.WhitePlayerId = await GetOrInsertPlayerAsync(surname.Trim(), forenames.Trim());
            }

            if (game.Tags.TryGetValue("black", out var blackValue))
            {
                PlayerNameParser.Parse(blackValue, out var surname, out var forenames);
                if (!string.IsNullOrWhiteSpace(surname))
                    game.BlackPlayerId = await GetOrInsertPlayerAsync(surname.Trim(), forenames.Trim());
            }
        }

        private async Task<int> GetOrInsertPlayerAsync(string surname, string forenames)
        {
            var forenamesNorm = forenames ?? string.Empty;
            var key = (surname, forenamesNorm);
            if (_cache.TryGetValue(key, out var id))
                return id;
            var existingId = await _chessRepository.GetPlayerIdBySurnameAndForenames(surname, forenamesNorm);
            if (existingId.HasValue)
            {
                _cache[key] = existingId.Value;
                return existingId.Value;
            }
            var sameSurname = await _chessRepository.GetPlayersBySurname(surname);
            foreach (var p in sameSurname)
            {
                if (PlayerForenamesMatcher.ForenamesMatch(p.Forenames, forenamesNorm))
                {
                    _cache[key] = p.Id;
                    return p.Id;
                }
            }
            var player = new Player { Surname = surname, Forenames = forenamesNorm };
            id = await _chessRepository.InsertPlayer(player);
            _cache[key] = id;
            return id;
        }
    }
}
