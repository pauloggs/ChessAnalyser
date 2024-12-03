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

            var sourceSquare = GetSourceSquare(
                previousBoardPosition,
                piece,
                ply,
                colour);

            var destinationSquare = GetDestinationSquare(ply);
            
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

            Console.WriteLine($"MoveInt > GetPiece: '{rawMove}'");

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
            }
            else if (char.IsUpper(firstChar))
            {
                piece = Constants.Pieces[firstChar]; // it is a non-pawn piece move
                ply.IsPieceMove = true;
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return piece;
        }

        private static int GetSourceSquare(
            BoardPosition previousBoardPosition,
            Piece piece,
            Ply ply,
            char colour)
        {
            // example Nc6, fxe5, Raf8, R1f8

            // if it's a pawn move, then
            //   if it's not a capture
            //      if it's a white move
            //          if the same file & (rank - 1)'s square = 1, then use this
            //          else if the same file & (rank -2)'s square = 1, then use this
            if (ply.IsPawnMove)
            {
                if (!ply.IsCapture)
                {

                }
            }
            return 0;
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
                var destinationFile = Constants.File[ply.RawMove[^2]];

                var destinationRank = ply.RawMove[^1];

                return destinationRank * 8 + (destinationFile - 1);
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
    }
}

