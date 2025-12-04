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
            Ply ply);
    }

    public class MoveInterpreterHelper(
        ISourceSquareHelper sourceSquareHelper,
        IDestinationSquareHelper destinationSquareHelper,
        IPawnMoveInterpreter pawnMoveInterpreter) : IMoveInterpreterHelper
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
                ply.Piece = Constants.Pieces['P'];
            }
            else if (char.IsUpper(firstChar))
            {
                piece = PieceRetriever.GetSafePiece(firstChar); // get the piece safely
                ply.IsPieceMove = true;
                ply.Piece = Constants.Pieces[firstChar];
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
            Ply ply)
        {
            // example Nc6, fxe5, Raf8, R1f8
            var sourceSquare = -1;

            var rankDirection = ply.Colour == Colour.W ? -1 : 1;

            //// TODO. Planned refactoring:
            //if (ply.IsPawnMove)
            //{
            //    // sourceSquare = pawnMoveInterpreter.GetSourceSquare();
            //}
            //else if (ply.IsPieceMove)
            //{
            //    // sourceSquare = pieceMoveInterpreter.GetSourceSquare();
            //}
            //else
            //{
            //    throw new Exception($"MoveInterpreter > GetSourceSquare: {ply.MoveNumber}, {ply.Colour}, {ply.RawMove} not a pawn or piece move");
            //}

            // if it's a pawn move, then
            //   if it's not a capture
            //      if it's a white move
            //          if the same file & (rank - 1)'s square = 1, then use this
            //          else if the same file & (rank -2)'s square = 1, then use this
            if (ply.IsPawnMove)
            {
                sourceSquare = pawnMoveInterpreter.GetSourceSquare(
                        previousBoardPosition,
                        ply);
            }
            else if (ply.IsPieceMove)
            {
                // Example Rhxh4, an example of a move where we need to differentiate between two rooks that can move to h4
                (int sourceRank, int sourceFile) = sourceSquareHelper.GetSourceRankAndOrFile(ply.RawMove);

                // must be N, B, R, Q ot K
                switch (ply.Piece.Name)
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
                                    ply.Piece,
                                    ply.Colour);


                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                sourceRank,
                                sourceFile) == true) break;
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
                                    ply.Piece,
                                    ply.Colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    sourceRank,
                                    sourceFile) == true) break;
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
                                    ply.Piece,
                                    ply.Colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    sourceRank,
                                    sourceFile) == true) break;
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
                                    ply.Piece,
                                    ply.Colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    sourceRank,
                                    sourceFile) == true) break;
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
                                        ply.Piece,
                                        ply.Colour);

                                    if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                        potentialSourceRank,
                                        potentialSourceFile,
                                        sourceRank,
                                        sourceFile) == true) break;
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
                                ply.Piece,
                                ply.Colour);

                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                sourceRank,
                                sourceFile) == true) break;
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
                                    ply.Piece   ,
                                    ply.Colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    sourceRank,
                                    sourceFile) == true) break;
                            }
                        }
                        break;
                }
            }

            return sourceSquare;
        }

    }
}
