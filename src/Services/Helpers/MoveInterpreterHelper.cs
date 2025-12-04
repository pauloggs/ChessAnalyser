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
        IPawnMoveInterpreter pawnMoveInterpreter,
        IPieceMoveInterpreter pieceMoveInterpreter) : IMoveInterpreterHelper
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
            var sourceSquare = Constants.MoveNotFound;

            if (ply.IsPawnMove)
            {
                sourceSquare = pawnMoveInterpreter.GetSourceSquare(
                        previousBoardPosition,
                        ply);
            }
            else if (ply.IsPieceMove)
            {
                sourceSquare = pieceMoveInterpreter.GetSourceSquare(
                        previousBoardPosition,
                        ply);
            }

            return sourceSquare;
        }

    }
}
