using Interfaces;
using Interfaces.DTO;
using Services.Helpers;

namespace ServicesTests.Helpers;

public class PieceRetrieverTests
{
    [Fact]
    public void GetSafePiece_ValidPieceChar_ReturnsCorrectPiece()
    {
        // Arrange
        char pieceChar = 'N'; // Knight

        // Act
        var piece = PieceRetriever.GetSafePiece(pieceChar);

        // Assert
        Assert.Equal('N', piece.Name);
        Assert.Equal(3.0, piece.Value); // Assuming knight value is 3.0
    }

    [Fact]
    public void GetSafePiece_InvalidPieceChar_ThrowsException()
    {
        // Arrange
        char pieceChar = 'Z'; // Invalid piece
        // Act & Assert
        Assert.Throws<Exception>(() => PieceRetriever.GetSafePiece(pieceChar));
    }

    [Fact]
    public void GetSafePiece_LowercasePieceChar_ThrowsException()
    {
        // Arrange
        char pieceChar = 'p'; // Lowercase pawn

        // Act & Assert
        Assert.Throws<Exception>(() => PieceRetriever.GetSafePiece(pieceChar));
    }

    [Fact]
    public void GetSafePiece_CastlingPieceChar_ReturnsCorrectPiece()
    {
        // Arrange
        char pieceChar = 'C'; // Castling

        // Act
        var piece = PieceRetriever.GetSafePiece(pieceChar);

        // Assert
        Assert.Equal('C', piece.Name);
        Assert.Equal(0.0, piece.Value); // Assuming castling value is 0.0
    }
}
