using Interfaces.DTO;
using Services.Helpers;

namespace Services
{
	public interface IMoveInterpreter
    {
        /// <summary>
        /// From move Raf8, find out the following
        /// 1 the piece
        /// 2 the source square (0-63)
        /// 3 the destination square (0-63)
        /// </summary>
        (Piece piece, int sourceSquare, int destinationSquare)
            GetSourceAndDestinationSquares(
            BoardPosition previousBoardPosition,
            Ply ply);
	}
    
	public class MoveInterpreter(IMoveInterpreterHelper moveInterpreterHelper) : IMoveInterpreter
    {
        public (Piece piece, int sourceSquare, int destinationSquare)
            GetSourceAndDestinationSquares(
                BoardPosition previousBoardPosition,
                Ply ply)
        {
            if (ply == null)
            {
                throw new ArgumentNullException(nameof(ply));
            }

            // basic validation
            if (string.IsNullOrEmpty(ply.RawMove) || ply.RawMove.Length < 2)
            {
                throw new Exception($"MoveInterpreter > GetSourceAndDestinationSquares: {ply.MoveNumber}, {ply.Colour}, {ply.RawMove} invalid move");
            }

            // remove any '+' at the end of the move
            moveInterpreterHelper.RemoveCheck(ply);

            // get the piece
            var piece = moveInterpreterHelper.GetPiece(ply);

            // get the destination square from the Ply (the last two characters of the move)
            var destinationSquare = moveInterpreterHelper.GetDestinationSquare(ply);

            // get the source square
            var sourceSquare = moveInterpreterHelper.GetSourceSquare(
                previousBoardPosition,
                ply);

            // return the piece, source square and destination square
            return (piece, sourceSquare, destinationSquare);
        }
    }
}

