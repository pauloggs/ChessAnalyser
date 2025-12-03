using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public interface IMoveInterpreterHelper
    {
        /// <summary>
        /// Removes the check indicator '+' from the raw move string of the given ply.
        /// </summary>
        /// <param name="ply"></param>
        void RemoveCheck(Ply ply);

        /// <summary>
        /// Gets the piece involved in the given ply based on its raw move string.
        /// </summary>
        /// <param name="ply"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        Piece GetPiece(Ply ply);

        /// <summary>
        /// Get the destination square (0-63) from the last two characters of
        /// the move, for example Nf6 takes 'f' and '6' to determine file and
        /// rank, and from these determine the destination square.
        /// If it is not a piece move (i.e. is castling), return -1.
        /// </summary>
        int GetDestinationSquare(Ply ply);

        /// <summary>
        /// Gets the source square (0-63) for the given ply based on the previous board position and the colour of the moving piece.
        /// </summary>
        /// <param name="previousBoardPosition"></param>
        /// <param name="ply"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        int GetSourceSquare(
            BoardPosition previousBoardPosition,
            Ply ply,
            Colour colour);
    }

    public class MoveInterpreterHelper(
        ISourceSquareHelper sourceSquareHelper,
        IDestinationSquareHelper destinationSquareHelper) : IMoveInterpreterHelper
    {        
        public void RemoveCheck(Ply ply)
        {
            if (ply.RawMove[^1] == '+')
            {
                ply.RawMove = ply.RawMove.Remove(ply.RawMove.Length - 1);
                ply.IsCheck = true;
            }
        }

        public Piece GetPiece(Ply ply)
        {
            var rawMove = ply.RawMove;

            var firstChar = rawMove[0];

            Piece piece;

            if (rawMove.ToLower().Contains('x'))
            {
                ply.IsCapture = true;
            }

            if (rawMove.Contains('='))
            {
                ply.IsPawnMove = true;
                ply.IsPromotion = true;
                var promotionPiece = (rawMove.Substring(rawMove.IndexOf('=') + 1))[0];
                piece = PieceRetriever.GetSafePiece(promotionPiece); // get the promotion piece safely
            }
            else if (rawMove == "O-O-O")
            {
                piece = Constants.Pieces['C']; // is a castling move
                ply.IsQueensideCastling = true;
            }
            else if (rawMove == "O-O")
            {
                piece = Constants.Pieces['C']; // is a castling move
                ply.IsKingsideCastling = true;
            }
            else if (char.IsLower(firstChar))
            {
                piece = Constants.Pieces['P']; // is a pawn move
                ply.IsPawnMove = true;
                ply.IsPieceMove = true;
                ply.Piece = 'P';
            }
            else if (char.IsUpper(firstChar))
            {
                piece = PieceRetriever.GetSafePiece(firstChar); // get the piece safely
                ply.IsPieceMove = true;
                ply.Piece = firstChar;
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return piece;
        }
       
        public int GetDestinationSquare(Ply ply)
        {
            return destinationSquareHelper.GetDestinationSquare(ply);
        }

        public int GetSourceSquare(
            BoardPosition previousBoardPosition,
            Ply ply,
            Colour colour)
        {
            // example Nc6, fxe5, Raf8, R1f8
            var sourceSquare = -1;

            var rankDirection = colour == Colour.W ? -1 : 1;

            // if it's a pawn move, then
            //   if it's not a capture
            //      if it's a white move
            //          if the same file & (rank - 1)'s square = 1, then use this
            //          else if the same file & (rank -2)'s square = 1, then use this
            if (ply.IsPawnMove)
            {
                if (!ply.IsCapture)
                {
                    // If it is not a capture, then the pawn must come from the same file,
                    // so we just need to check one or two ranks backward or forward depending on colour

                    // check the rank above (B) or below (W), for a 1-space pawn move
                    sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    ply.DestinationRank + rankDirection,
                                    ply.DestinationFile,
                                    'P',
                                    colour);

                    // if not found, check for a 2-space pawn move - i.e. from starting position
                    if (sourceSquare < 0)
                    {
                        sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    ply.DestinationRank + 2 * rankDirection,
                                    ply.DestinationFile,
                                    'P',
                                    colour);

                        if (sourceSquare < 0)
                        {
                            throw new Exception($"MoveInterpreter > GetSourceSquare: {ply.MoveNumber}, {colour}, {ply.RawMove} no source square found");
                        }
                    }
                    ;
                }
                else
                {
                    // find the file before the 'x'
                    string[] rawMoveSplit = ply.RawMove.ToLower().Split('x');
                    var sourceFileKey = rawMoveSplit[0];
                    var sourceFile = Constants.File[sourceFileKey[0]];
                    var sourceRank = ply.DestinationRank + rankDirection;
                    sourceSquare = sourceRank * 8 + sourceFile;
                }
            }
            else if (ply.IsPieceMove)
            {
                // check if there is more information on the source square, e.g. Raf8 indicates the rook comes from the a file

                var specifiedFile = -1;
                var specifiedRank = -1;

                // str.Replace(c, string.Empty);

                if (ply.RawMove.ToLower().Replace("x", string.Empty).Length >= 4)
                {
                    var sourceRankOrFile = ply.RawMove[1];
                    if (char.IsNumber(sourceRankOrFile))
                    {
                        specifiedRank = (int)sourceRankOrFile;
                    }
                    else
                    {
                        specifiedFile = Constants.File[sourceRankOrFile];
                    }
                }

                // must be N, B, R, Q ot K
                switch (ply.Piece)
                {
                    case 'N':
                        // get the eight possible squares the original N may have come from
                        //  1. DR -2, DF - 1
                        foreach (var np in Constants.RelativeKnightPositions)
                        {
                            var potentialSourceFile = ply.DestinationFile + np.file;
                            var potentialSourceRank = ply.DestinationRank + np.rank;

                            sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'N',
                                    colour);


                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                specifiedRank,
                                specifiedFile) == true) break;
                        }
                        break;
                    case 'B':
                        for (var diagDist = 1; diagDist < 8; diagDist++)
                        {
                            for (var dir = 0; dir < 4; dir++)
                            {
                                var fileAdj = diagDist * (2 * (dir / 2) - 1); // (dir/2)*2 - 1
                                var rankAdj = diagDist * (2 * (dir % 2) - 1); // (dir%2)*2 - 1
                                var potentialSourceFile = ply.DestinationFile + fileAdj;
                                var potentialSourceRank = ply.DestinationRank + rankAdj;

                                sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'B',
                                    colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    specifiedRank,
                                    specifiedFile) == true) break;
                            }
                            if (sourceSquare >= 0) break;
                        }
                        break;
                    case 'R':
                        for (var orthoDist = 1; orthoDist < 8; orthoDist++)
                        {
                            for (var dir = 0; dir < 4; dir++)
                            {
                                int fileAdj = orthoDist * (dir % 2) * (dir - 2); // (dir%2)*(dir-2)
                                int rankAdj = orthoDist * ((dir + 1) % 2) * (dir - 1); // ((dir+1)%2)*(dir-1)
                                var potentialSourceFile = ply.DestinationFile + fileAdj;
                                var potentialSourceRank = ply.DestinationRank + rankAdj;

                                sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'R',
                                    colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    specifiedRank,
                                    specifiedFile) == true) break;
                            }
                            if (sourceSquare >= 0) break;
                        }
                        break;
                    case 'Q':
                        // loop through diagonals (Bishop code)
                        for (var diagDist = 1; diagDist < 8; diagDist++)
                        {
                            for (var dir = 0; dir < 4; dir++)
                            {
                                var fileAdj = diagDist * (2 * (dir / 2) - 1);
                                var rankAdj = diagDist * (2 * (dir % 2) - 1);
                                var potentialSourceFile = ply.DestinationFile + fileAdj;
                                var potentialSourceRank = ply.DestinationRank + rankAdj;

                                sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'Q',
                                    colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    specifiedRank,
                                    specifiedFile) == true) break;
                            }
                            if (sourceSquare >= 0) break;
                        }
                        if (sourceSquare < 0)
                        {
                            for (var orthoDist = 1; orthoDist < 8; orthoDist++)
                            {
                                for (var dir = 0; dir < 4; dir++)
                                {
                                    int fileAdj = orthoDist * (dir % 2) * (dir - 2); // (dir%2)*(dir-2)
                                    int rankAdj = orthoDist * ((dir + 1) % 2) * (dir - 1); // ((dir+1)%2)*(dir-1)
                                    var potentialSourceFile = ply.DestinationFile + fileAdj;
                                    var potentialSourceRank = ply.DestinationRank + rankAdj;

                                    sourceSquare = sourceSquareHelper.GetSourceSquare(
                                        previousBoardPosition,
                                        potentialSourceRank,
                                        potentialSourceFile,
                                        'Q',
                                        colour);

                                    if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                        potentialSourceRank,
                                        potentialSourceFile,
                                        specifiedRank,
                                        specifiedFile) == true) break;
                                }
                                if (sourceSquare >= 0) break;
                            }
                        }


                        break;
                    case 'K':
                        for (var dir = 0; dir < 4; dir++)
                        {
                            var fileAdj = (2 * (dir / 2) - 1); // (dir/2)*2 - 1
                            var rankAdj = (2 * (dir % 2) - 1); // (dir%2)*2 - 1
                            var potentialSourceFile = ply.DestinationFile + fileAdj;
                            var potentialSourceRank = ply.DestinationRank + rankAdj;

                            sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                'K',
                                colour);

                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                specifiedRank,
                                specifiedFile) == true) break;
                        }
                        if (sourceSquare < 0)
                        {
                            for (var dir = 0; dir < 4; dir++)
                            {
                                int fileAdj = (dir % 2) * (dir - 2); // (dir%2)*(dir-2)
                                int rankAdj = ((dir + 1) % 2) * (dir - 1); // ((dir+1)%2)*(dir-1)
                                var potentialSourceFile = ply.DestinationFile + fileAdj;
                                var potentialSourceRank = ply.DestinationRank + rankAdj;

                                sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'K',
                                    colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    specifiedRank,
                                    specifiedFile) == true) break;
                            }
                        }
                        break;
                }
            }

            return sourceSquare;
        }

    }
}
