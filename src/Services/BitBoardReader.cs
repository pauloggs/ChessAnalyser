using Interfaces;
using Interfaces.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Interfaces.Constants;

namespace Services
{
    public static class BitBoardReader
    {
        public static (Piece? piece, Colour? colour) ReadSquare(
            BoardPosition boardPosition,
            int square // 0-63
            )
        {
            var occupyingPieceKey = 'X';

            var occupyingColour = Colour.N;

            var occupyingPieceCount = 0;

            var actualColours = Enum.GetValues<Colour>().Where(c => c != Colour.N);

            foreach (var colour in actualColours)
            {
                foreach (var pieceKey in Constants.PieceIndex.Keys)
                {
                    var piecePositionsKey = colour.ToString() + pieceKey;
                    var piecePositions = boardPosition.PiecePositions[piecePositionsKey];

                    if ((piecePositions & (1ul << square)) != 0)
                    {
                        occupyingPieceCount++;
                        if (occupyingPieceCount > 1)
                        {
                            throw new InvalidOperationException($"More than one piece found at square {square.Algebraic()}");
                        }
                        occupyingPieceKey = pieceKey;
                        occupyingColour = colour;
                    }
                }
            }

            if (occupyingPieceCount == 0)
            {
                return (null, null); // No piece found at the square
            }

            return (Constants.Pieces[occupyingPieceKey], occupyingColour);
        }
    }
}
