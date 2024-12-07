using Interfaces;
using Interfaces.DTO;

namespace Services
{
	public interface IMoveInterpreter
    {
        (Piece piece, int sourceSquare, int destinationSquare)
            GetSourceAndDestinationSquares(
            BoardPosition previousBoardPosition,
            Ply ply,
            char colour);
	}   

    /// <summary>
    /// From move Raf8, find out the following
    /// 1 the piece
    /// 2 the source square (0-63)
    /// 3 the destination square (0-63)
    /// </summary>
	public class MoveInterpreter(IBitBoardManipulator bitBoardManipulator) : IMoveInterpreter
    {
        private readonly IBitBoardManipulator _bitBoardManipulator = bitBoardManipulator;

        public (Piece piece, int sourceSquare, int destinationSquare)
            GetSourceAndDestinationSquares(
                BoardPosition previousBoardPosition,
                Ply ply,
                char colour)
        {
            if (string.IsNullOrEmpty(ply.RawMove) || ply.RawMove.Length < 2)
            {
                return (new Piece(), -1, -1);
            }

            RemoveCheck(ply);

            var piece = GetPiece(ply);

            var destinationSquare = GetDestinationSquare(ply);

            var sourceSquare = GetSourceSquare(
                previousBoardPosition,
                ply,
                colour);            
            
            return (piece, sourceSquare, destinationSquare);
        }

        /// <summary>
        /// If the Ply's RawMove contains '+' at the end, remove it.
        /// </summary>
        /// <param name="rawMove"></param>
        /// <returns></returns>
        private static void RemoveCheck(Ply ply)
        {
            if (ply.RawMove[^1] == '+')
            {
                ply.RawMove = ply.RawMove.Remove(ply.RawMove.Length - 1);
                ply.IsCheck = true;
            }
        }


        /// <summary>
        /// Get the <see cref="Piece"/> from Ply.RawMove, and set
        /// flags such as castling, ischeck, is capture, on the Ply object.
        /// </summary>
        /// <param name="ply"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Piece GetPiece(Ply ply)
        {
            var rawMove = ply.RawMove;

            var firstChar = rawMove[0];

            Piece piece;

            if (rawMove.ToLower().Contains('x'))
            {
                ply.IsCapture = true;
            }


            if (rawMove == "O-O-O")
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
                piece = Constants.Pieces[firstChar]; // it is a non-pawn piece move
                ply.IsPieceMove = true;
                ply.Piece = firstChar;
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return piece;
        }

        private int GetSourceSquare(
            BoardPosition previousBoardPosition,
            Ply ply,
            char colour)
        {
            // example Nc6, fxe5, Raf8, R1f8
            var sourceSquare = -1;

            var multiplier = colour == 'W' ? -1 : 1;

            // if it's a pawn move, then
            //   if it's not a capture
            //      if it's a white move
            //          if the same file & (rank - 1)'s square = 1, then use this
            //          else if the same file & (rank -2)'s square = 1, then use this
            if (ply.IsPawnMove)
            {
                if (!ply.IsCapture)
                {
                    // check the rank above (B) or below (W)
                    sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    ply.DestinationRank + multiplier,
                                    ply.DestinationFile,
                                    'P',
                                    colour);

                    if (sourceSquare < 0)
                    {
                        sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    ply.DestinationRank + 2 * multiplier,
                                    ply.DestinationFile,
                                    'P',
                                    colour);

                        if (sourceSquare < 0)
                        {
                            throw new Exception($"MoveInterpreter > GetSourceSquare: {ply.MoveNumber}, {colour}, {ply.RawMove} no source square found");
                        }
                    };
                }
                else
                {
                    // find the file before the 'x'
                    string[] rawMoveSplit = ply.RawMove.ToLower().Split('x');
                    var sourceFileKey = rawMoveSplit[0];
                    var sourceFile = Constants.File[sourceFileKey[0]];
                    var sourceRank = ply.DestinationRank + multiplier;
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

                            sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'N',
                                    colour);


                            if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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
                                var fileAdj = diagDist * (2 * (dir / 2) - 1);
                                var rankAdj = diagDist * (2 * (dir % 2) - 1);
                                var potentialSourceFile = ply.DestinationFile + fileAdj;
                                var potentialSourceRank = ply.DestinationRank + rankAdj;

                                sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'B',
                                    colour);

                                if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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

                                sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'R',
                                    colour);

                                if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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

                                sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'Q',
                                    colour);

                                if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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

                                    sourceSquare = GetSourceSquare(
                                        previousBoardPosition,
                                        potentialSourceRank,
                                        potentialSourceFile,
                                        'Q',
                                        colour);

                                    if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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
                            int fileAdj = (dir % 2) * (dir - 2); // %2 x (dir - 2)
                            int rankAdj = ((dir + 1) % 2) * (dir - 2); // (dir+1)%2 x (dir-2)
                            var potentialSourceFile = ply.DestinationFile + fileAdj;
                            var potentialSourceRank = ply.DestinationRank + rankAdj;

                            sourceSquare = GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                'K',
                                colour);

                            if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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

                                sourceSquare = GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    'K',
                                    colour);

                                if (sourceSquare >= 0 && PotentialRankOrFileMatchesSpecifiedRankOrFile(
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

        /// <summary>
        /// Get the destination square (0-63) from the last two characters of
        /// the move, for example Nf6 takes 'f' and '6' to determine file and
        /// rank, and from these determine the destination square.
        /// If it is not a piece move (i.e. is castling), return -1.
        /// </summary>
        private static int GetDestinationSquare(Ply ply)
        {          
            if (ply.IsPieceMove)
            {
                ply.DestinationFile = Constants.File[ply.RawMove[^2]]; ;

                ply.DestinationRank = (int)char.GetNumericValue(ply.RawMove[ply.RawMove.Length - 1]) - 1;

                return (ply.DestinationRank * 8) + ply.DestinationFile;
            }
            else if (ply.IsKingsideCastling || ply.IsQueensideCastling)
            {
                // Can't get single destination square for a castling move - this is handled elsewhere
                return -1;
            }
            else
            {
                throw new Exception("Move must be either a piece move or casteling.");
            }
        }

        /// <summary>
        /// Check if the specified colour/piece is at the specified rank/file.
        /// Return the 0-63 square number if present, or -1 if not.
        /// </summary>
        /// <param name="previousBoardPosition"></param>
        /// <param name="potentialSourceRank"></param>
        /// <param name="potentialSourceFile"></param>
        /// <param name="piece"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        private int GetSourceSquare(
            BoardPosition previousBoardPosition,
            int potentialSourceRank,
            int potentialSourceFile,
            char piece,
            char colour)
        {
            var returnValue = -1;

            if (potentialSourceFile >= 0 && potentialSourceFile < 8
                 && potentialSourceRank >= 0 && potentialSourceRank < 8)
            {
                // check if the piece is here
                var potentialKnightSquare = _bitBoardManipulator.ReadSquare(
                    previousBoardPosition,
                    piece,
                    colour,
                    potentialSourceRank,
                    potentialSourceFile);

                if (potentialKnightSquare == true)
                {
                    returnValue = ExtensionMethods.GetSquareFromRankAndFile(potentialSourceRank, potentialSourceFile);
                }
            }

            return returnValue;
        }

        private bool PotentialRankOrFileMatchesSpecifiedRankOrFile(
            int potentialRank,
            int potentialFile,
            int specifiedRank,
            int specifiedFile)
        {
            if (specifiedRank < 0 && specifiedFile < 0) return true; // matches if nothing specified

            if (potentialRank == specifiedRank
                || potentialFile == specifiedFile) return true;

            return false;

            //return true;
        }
    }
}

