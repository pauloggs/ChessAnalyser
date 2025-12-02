using Interfaces;
using Interfaces.DTO;
using Services.Helpers;

namespace ServicesHelpersTests
{
    public class MoveInterpreterHelperTests
    {
        [Theory]
        [InlineData("e4+", "e4")]
        [InlineData("Nf3+", "Nf3")]
        [InlineData("O-O-O+", "O-O-O")]
        public void RemoveCheck_ShouldRemoveCheckIndicatorAndSetIsCheckTrue(string rawMove, string newRawMove)
        {
            // Arrange
            var ply = new Interfaces.DTO.Ply
            {
                RawMove = rawMove,
                IsCheck = false
            };

            // Act
            MoveInterpreterHelper.RemoveCheck(ply);

            // Assert
            Assert.Equal(newRawMove, ply.RawMove);
            Assert.True(ply.IsCheck);
        }

        [Theory]
        [InlineData("e4")]
        [InlineData("Nf3")]
        [InlineData("O-O-O")]
        public void RemoveCheck_ShouldNotModifyMoveWithoutCheckIndicator(string rawMove)
        {
            // Arrange
            var ply = new Interfaces.DTO.Ply
            {
                RawMove = rawMove,
                IsCheck = false
            };

            MoveInterpreterHelper.RemoveCheck(ply);

            // Assert
            Assert.Equal(rawMove, ply.RawMove);
        }

        [Fact]
        public void GetPiece_ShouldIdentifyPieceCorrectly()
        {
            // Arrange
            var plyPawn = new Ply { RawMove = "e4" };
            var plyKnight = new Ply { RawMove = "Nf3" };
            var plyCapture = new Ply { RawMove = "Bxe5" };
            var plyPromotion = new Ply { RawMove = "e8=Q" };
            var plyKingsideCastling = new Ply { RawMove = "O-O" };
            var plyQueensideCastling = new Ply { RawMove = "O-O-O" };

            // Act
            var piecePawn = MoveInterpreterHelper.GetPiece(plyPawn);
            var pieceKnight = MoveInterpreterHelper.GetPiece(plyKnight);
            var pieceCapture = MoveInterpreterHelper.GetPiece(plyCapture);
            var piecePromotion = MoveInterpreterHelper.GetPiece(plyPromotion);
            var pieceKingsideCastling = MoveInterpreterHelper.GetPiece(plyKingsideCastling);
            var pieceQueensideCastling = MoveInterpreterHelper.GetPiece(plyQueensideCastling);

            // Assert
            Assert.Equal('P', piecePawn.Name);
            Assert.Equal('N', pieceKnight.Name);
            Assert.Equal('B', pieceCapture.Name);
            Assert.Equal('Q', piecePromotion.Name);
            Assert.Equal('C', pieceKingsideCastling.Name);
            Assert.Equal('C', pieceQueensideCastling.Name);
            Assert.False(plyPawn.IsCapture);
            Assert.True(plyCapture.IsCapture);
            Assert.True(plyPromotion.IsPromotion);
            Assert.True(plyKingsideCastling.IsKingsideCastling);
            Assert.True(plyQueensideCastling.IsQueensideCastling);
        }

        [Fact]
        public void GetPiece_ShouldThrowExceptionForInvalidMove()
        {
            // Arrange
            var plyInvalid = new Ply { RawMove = "InvalidMove" };

            // Act & Assert
            Assert.Throws<Exception>(() => MoveInterpreterHelper.GetPiece(plyInvalid));
        }

        [Fact]
        public void GetPiece_ShouldReturnPawn_WhenRawMoveIsLowercase()
        {
            // Arrange
            var ply = new Ply { RawMove = "a" }; // Represents a pawn move
            Constants.Pieces['P'] = new Piece(name: 'P', value: 0.0); // Setup the pawn in Constants

            // Act
            var piece = MoveInterpreterHelper.GetPiece(ply);

            // Assert
            Assert.NotNull(piece);
            Assert.Equal(Constants.Pieces['P'], piece);
            Assert.True(ply.IsPawnMove);
            Assert.True(ply.IsPieceMove);
            Assert.Equal('P', ply.Piece);
        }

        [Fact]
        public void GetPiece_ShouldReturnPromotedPiece_WhenRawMoveIsPromotion()
        {
            // Arrange
            var ply = new Ply { RawMove = "e8=Q" }; // Represents a promotion move
            var expectedPiece = new Piece(name: 'P', value: 0.0);
            Constants.Pieces['Q'] = expectedPiece; // Setup the promoted piece (Queen)

            // Act
            var piece = MoveInterpreterHelper.GetPiece(ply);

            // Assert
            Assert.Equal(expectedPiece, piece);
            Assert.True(ply.IsPawnMove);
            Assert.True(ply.IsPromotion);
        }

        [Fact]
        public void GetPiece_ShouldReturnKingsideCastling_WhenRawMoveIsKingsideCastling()
        {
            // Arrange
            var ply = new Ply { RawMove = "O-O" }; // Represents kingside castling
            Constants.Pieces['C'] = new Piece(name: 'C', value: 0.0); // Setup the castling piece

            // Act
            var piece = MoveInterpreterHelper.GetPiece(ply);

            // Assert
            Assert.Equal(Constants.Pieces['C'], piece);
            Assert.True(ply.IsKingsideCastling);
        }

        [Fact]
        public void GetPiece_ShouldReturnQueensideCastling_WhenRawMoveIsQueensideCastling()
        {
            // Arrange
            var ply = new Ply { RawMove = "O-O-O" }; // Represents queenside castling
            Constants.Pieces['C'] = new Piece(name: 'C', value: 0.0); // Setup the castling piece

            // Act
            var piece = MoveInterpreterHelper.GetPiece(ply);

            // Assert
            Assert.Equal(Constants.Pieces['C'], piece);
            Assert.True(ply.IsQueensideCastling);
        }

        [Fact]
        public void GetPiece_ShouldReturnNonPawnPiece_WhenRawMoveStartsWithUppercase()
        {
            // Arrange
            var ply = new Ply { RawMove = "Nf3" }; // Represents a Knight move
            var expectedPiece = new Piece(name: 'N', value: 0.0);
            Constants.Pieces['N'] = expectedPiece; // Assuming Knight setup in Constants

            // Act
            var piece = MoveInterpreterHelper.GetPiece(ply);

            // Assert
            Assert.Equal(expectedPiece, piece);
            Assert.True(ply.IsPieceMove);
            Assert.Equal('N', ply.Piece);
        }

        [Fact]
        public void GetPiece_ShouldThrowException_WhenRawMoveIsInvalid()
        {
            // Arrange
            var ply = new Ply { RawMove = "InvalidMove" }; // Invalid move

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => MoveInterpreterHelper.GetPiece(ply));
            Assert.Equal("Unable to retrieve Piece from key: 'I'", exception.Message);
        }

        [Fact]
        public void GetPiece_ShouldThrowException_WhenPieceKeyNotFound()
        {
            // Arrange
            var ply = new Ply { RawMove = "Z4" }; // Invalid character not defined

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => MoveInterpreterHelper.GetPiece(ply));
            Assert.Contains("Unable to retrieve Piece from key:", exception.Message);
        }
    }
}
