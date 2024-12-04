namespace Services
{
    using System;
    using Interfaces;
    using Interfaces.DTO;

    public interface IDisplayService
    {
        void DisplayBoardPosition(sbyte[,] boardArray);

        void DisplayBoardPosition(BoardPosition boardPosition);

        sbyte[,] GetBoardArrayFromBoardPositions(BoardPosition boardPosition);
    }

    public class DisplayService : IDisplayService
    {
        public void DisplayBoardPosition(sbyte[,] boardArray)
        {
            for (var rankIndex = 7; rankIndex >= 0; rankIndex--)
            {
                Console.Write($"{rankIndex + 1}|");
                for (var fileIndex = 7; fileIndex >= 0; fileIndex--)
                {
                    Console.Write($"{(Constants.DisplayBoardPiece)boardArray[rankIndex, fileIndex]}|");
                }
                Console.WriteLine();
            }
            Console.Write(" |");
            for (var fileIndex = 7; fileIndex >= 0; fileIndex--)
            {
                Console.Write($"{Constants.FileIds[fileIndex]}|");
            }
            Console.WriteLine();
        }

        public void DisplayBoardPosition(BoardPosition boardPosition)
        {
            var boardArray = GetBoardArrayFromBoardPositions(boardPosition);

            DisplayBoardPosition(boardArray);
        }

        public sbyte[,] GetBoardArrayFromBoardPositions(BoardPosition boardPosition)
        {
            var boardArray = new sbyte[8, 8];

            foreach (var key in boardPosition.PiecePositions.Keys)
            {
                var col = key[0] == 'W' ? 1 : 1;

                var piece = key[1].ToString();

                var piecePositions = boardPosition.PiecePositions[key];

                byte[] ranks = BitConverter.GetBytes(piecePositions);

                for (var rankIndex = 0; rankIndex < 8; rankIndex++)
                {
                    var rank = ranks[rankIndex];

                    string files = Convert.ToString(rank, 2).PadLeft(8, '0');

                    for (var fileIndex = 7; fileIndex >= 0; fileIndex--)
                    {
                        var pieceSbyte
                            = (sbyte)((int)(Constants.DisplayBoardPiece)Enum
                            .Parse(typeof(Constants.DisplayBoardPiece), piece)
                            * col);

                        if (files[fileIndex] == '1') boardArray[rankIndex, fileIndex] = pieceSbyte;
                    }
                }
            }

            return boardArray;
        }
    }
}