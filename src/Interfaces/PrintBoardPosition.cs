using Interfaces.DTO;

namespace Interfaces;

public class PrintBoardPosition
{
    // ANSI codes for 2026 terminals
    private const string White = "\u001b[97;1m"; // Bold Bright White
    private const string Black = "\u001b[36m";   // Cyan (high contrast)
    private const string Reset = "\u001b[0m";
    private const string Empty = "\u001b[90m";   // Dark Gray

    public static void Print(BoardPosition pos)
    {
        // Ensure the terminal supports UTF-8 for chess symbols if needed
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write($"{rank + 1} ");
            for (int file = 0; file < 8; file++)
            {
                char piece = GetPieceAtSquare(pos, rank * 8 + file);

                // Determine color based on piece casing
                string color = piece == '.' ? Empty :
                               char.IsUpper(piece) ? White : Black;

                Console.Write($"{color}{piece} {Reset}");
            }
            Console.WriteLine();
        }
    }

    private static char GetPieceAtSquare(BoardPosition pos, int square)
    {
        ulong mask = 1ul << square;

        // Check each of your 12 bitboards
        if ((pos.PiecePositions["WP"] & mask) != 0) return 'P';
        if ((pos.PiecePositions["WN"] & mask) != 0) return 'N';
        if ((pos.PiecePositions["WB"] & mask) != 0) return 'B';
        if ((pos.PiecePositions["WR"] & mask) != 0) return 'R';
        if ((pos.PiecePositions["WQ"] & mask) != 0) return 'Q';
        if ((pos.PiecePositions["WK"] & mask) != 0) return 'K';

        if ((pos.PiecePositions["BP"] & mask) != 0) return 'p';
        if ((pos.PiecePositions["BN"] & mask) != 0) return 'n';
        if ((pos.PiecePositions["BB"] & mask) != 0) return 'b';
        if ((pos.PiecePositions["BR"] & mask) != 0) return 'r';
        if ((pos.PiecePositions["BQ"] & mask) != 0) return 'q';
        if ((pos.PiecePositions["BK"] & mask) != 0) return 'k';

        return '.'; // Empty square
    }
};


