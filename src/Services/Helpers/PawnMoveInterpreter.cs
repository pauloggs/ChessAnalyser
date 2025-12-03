using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public interface IPawnMoveInterpreter
    {
        int GetSourceSquare(
            BoardPosition previousBoardPosition,
            Ply ply);
    }

    public class PawnMoveInterpreter(
        ISourceSquareHelper sourceSquareHelper,
        IBitBoardManipulator bitBoardManipulator) : IPawnMoveInterpreter
    {
        public int GetSourceSquare(BoardPosition previousBoardPosition, Ply ply)
        {
            var rankDirection = ply.Colour == Colour.W ? -1 : 1;

            if (!ply.IsCapture)
            {
                // check one square then two squares up/down the file for a source pawn
                for (var rankOffset = 1; rankOffset <= 2; rankOffset++)
                {
                    var potentialSourceRank = ply.DestinationRank + (rankDirection * rankOffset);
                    var potentialSourceFile = ply.DestinationFile;

                    var sourcePawnFoundAtSquare = bitBoardManipulator.ReadSquare(
                        previousBoardPosition,
                        ply.Piece,
                        ply.Colour,
                        potentialSourceRank,
                        potentialSourceFile);

                    if (sourcePawnFoundAtSquare)
                    {
                        return SquareHelper.GetSquareFromRankAndFile(potentialSourceRank, potentialSourceFile);

                    }
                }

                // if still not found, throw exception
                throw new Exception($"MoveInterpreter > GetSourceSquare: {ply.MoveNumber}, {ply.Colour}, {ply.RawMove} no source square found");
            }
            else if (ply.IsCapture)
            {

            }

            throw new ArgumentException("Unable to determine source square for pawn move.");
        }
    }
}
