﻿using Interfaces;
using Interfaces.DTO;
using System.Text.RegularExpressions;

namespace Services
{
    public interface IParser
    {
        /// <summary>
        /// Gets raw games from the provided PGN file.
        /// </summary>
        /// <param name="rawPgn"></param>
        /// <returns></returns>
        List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn);

        List<Game> GetGamesFromRawPgns(List<RawPgn> rawPgns);

        void SetBoardPositions(List<Game> games);
    }

    public class Parser(INaming naming, IBoardPositionGenerator boardPositionGenerator) : IParser
    {
        private readonly INaming naming = naming;

        public Dictionary<string, string> GetGameTags(string rawGameContent)
        {
            var gameTags = new Dictionary<string, string>();

            foreach (var tag in Constants.GameTagIdentifiers)

            {
                gameTags[tag] = GetTag(tag, rawGameContent);
            }

            return gameTags;
        }

        public List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn)
        {
            var rawGames = new List<RawGame>();

            if (!rawPgn.Contents.Contains(Constants.GameStartMarker)) { return rawGames; }

            var pgnFileName = rawPgn.Name;

            string[] tokens = rawPgn.Contents.Split(new[] { Constants.GameStartMarker }, StringSplitOptions.None);
            
            foreach (string token in tokens)
            {
                if (token.Length > 0)
                {                  
                    var gameContents = (Constants.GameStartMarker + token).Trim();

                    rawGames.Add(new RawGame()
                    {
                        ParentPgnFileName = pgnFileName,
                        Contents = gameContents
                    });
                }
            }
            
            return rawGames;
        }

        private static string GetTag(string tag, string gameContents)
        {
            var startLocationOfTag = gameContents.IndexOf(tag);
            if (startLocationOfTag == -1) { return tag; }

            var contentsStartingWithTag = gameContents.Substring(startLocationOfTag + tag.Length);

            var endOfTagLocation = contentsStartingWithTag.IndexOf("]");

            var tagValue = contentsStartingWithTag.Substring(0, endOfTagLocation).Replace("\"", "").Trim();

            if (string.IsNullOrWhiteSpace(tagValue)) tagValue = Constants.DefaultEmptyTagValue;

            return tagValue;
        }     

        public List<Game> GetGamesFromRawPgns(List<RawPgn> rawPgns)
        {
            var games = new List<Game>();

            foreach (var rawPgn in rawPgns)
            {
                var rawGames = GetRawGamesFromPgnFile(rawPgn);

                foreach (var rawGame in rawGames)
                {
                    var rawPgnContent = rawGame.Contents;

                    var rawGameLines = Regex.Split(rawPgnContent, "\r\n|\r|\n");

                    var plyNumber = 1;

                    var tagDictionary = new Dictionary<string, string>();

                    var plyDictionary = new Dictionary<int, Ply>();

                    foreach (var rawline in rawGameLines)
                    {
                        string line = rawline.Trim();

                        if (string.IsNullOrWhiteSpace(line)) continue;

                        if (line.StartsWith("["))
                        {
                            AddGameTage(tagDictionary, line);
                        }
                        else
                        {
                            AddPlies(plyDictionary, line, ref plyNumber);
                        }
                    }

                    var gameName = GetGameName(tagDictionary);

                    var game = new Game() 
                    {
                        Name = gameName,
                        Tags = tagDictionary,
                        Plies = plyDictionary
                    };

                    games.Add(game);
                }
            }                
            
            return games;
        }

        private void AddGameTage(Dictionary<string, string> tagDictionary, string line)
        {
            var tagSections = line.Split(" ", 2);

            string tagKey = tagSections[0].Trim('[').ToLower();

            string tagValue = tagSections[1].Trim(']').Replace("\"","");

            tagDictionary[tagKey] = tagValue;
        }

        private static void AddPlies(Dictionary<int,Ply> plyDictionary, string line, ref int plyNumber)
        {
            Regex moveNumbersRegex = new Regex(@"\d+\.");

            line = moveNumbersRegex.Replace(line, "");

            var plies = line.Split(" ");

            foreach (string plyString in plies)
            {
                var ply = new Ply()
                {
                    MoveNumber = (plyNumber - 1) / 2 + 1,
                    Move = plyString
                };

                plyDictionary[plyNumber] = ply;

                plyNumber++;
            }
        }

        private static string GetGameName(Dictionary<string, string> tagDicionary)
        {
            var tagList = new List<string>();

            foreach (var tag in Constants.GameTagIdentifiers)
            {
                if (tagDicionary.ContainsKey(tag))
                {
                    tagList.Add(tagDicionary[tag]);
                }
                else
                {
                    tagList.Add(Constants.DefaultEmptyTagValue);
                }
            }

            return string.Join("|", tagList);
        }

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                game.BoardPositions[0] = boardPositionGenerator.GetStartingBoardPosition();

                foreach (var ply in game.Plies)
                {
                    var previousBoardPosition = game.BoardPositions[ply.Key-1];

                    var boardPosition
                        = boardPositionGenerator.GetBoardPositionFromMove(previousBoardPosition, ply.Value.Move);

                    game.BoardPositions[ply.Key] = boardPosition;
                }
            }
        }
    }
}
