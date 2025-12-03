using Interfaces.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public interface IPawnMoveInterpreter
    {
        int GetSourceSquare(
            BoardPosition previousBoardPosition,
            Ply ply);
    }

    public class PawnMoveInterpreter(ISourceSquareHelper sourceSquareHelper) : IPawnMoveInterpreter
    {
        public int GetSourceSquare(BoardPosition previousBoardPosition, Ply ply)
        {
            var rankDirection = ply.Colour == Colour.W ? -1 : 1;

            if (!ply.IsCapture)
            {
                // one or two square pawn move on the same file

                // check the rank above (B) or below (W), to see if the pawn came from there
                var sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                ply.DestinationRank + rankDirection,
                                ply.DestinationFile,
                                'P',
                                ply.Colour);
            }
            else if (ply.IsCapture)
            {

            }

            throw new ArgumentException("Unable to determine source square for pawn move.");
        }
    }
}
