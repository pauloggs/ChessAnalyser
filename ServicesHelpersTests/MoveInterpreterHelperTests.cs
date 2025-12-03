using Interfaces;
using Interfaces.DTO;
using Moq;
using Services.Helpers;

namespace ServicesHelpersTests
{
    public class MoveInterpreterHelperTests
    {
        private Mock<ISourceSquareHelper> sourceSquareHelperMock;
        private Mock<IDestinationSquareHelper> destinationSquareHelperMock;
        private IMoveInterpreterHelper sut;

        public MoveInterpreterHelperTests()
        {
            sourceSquareHelperMock = new Mock<ISourceSquareHelper>();
            destinationSquareHelperMock = new Mock<IDestinationSquareHelper>();
            sut = new MoveInterpreterHelper(sourceSquareHelperMock.Object, destinationSquareHelperMock.Object);
        }

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
            sut.RemoveCheck(ply);

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

            sut.RemoveCheck(ply);

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
            var piecePawn = sut.GetPiece(plyPawn);
            var pieceKnight = sut.GetPiece(plyKnight);
            var pieceCapture = sut.GetPiece(plyCapture);
            var piecePromotion = sut.GetPiece(plyPromotion);
            var pieceKingsideCastling = sut.GetPiece(plyKingsideCastling);
            var pieceQueensideCastling = sut.GetPiece(plyQueensideCastling);

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
            Assert.Throws<Exception>(() => sut.GetPiece(plyInvalid));
        }

        [Fact]
        public void GetPiece_ShouldReturnPawn_WhenRawMoveIsLowercase()
        {
            // Arrange
            var ply = new Ply { RawMove = "a" }; // Represents a pawn move

            // Act
            var piece = sut.GetPiece(ply);

            // Assert
            Assert.NotNull(piece);
            Assert.Equal(Constants.Pieces['P'], piece);
            Assert.True(ply.IsPawnMove);
            Assert.True(ply.IsPieceMove);
            Assert.Equal('P', ply.Piece);
        }

        [Fact]
        public void GetPiece_ShouldThrowException_WhenRawMoveIsInvalid()
        {
            // Arrange
            var ply = new Ply { RawMove = "InvalidMove" }; // Invalid move

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => sut.GetPiece(ply));
            Assert.Equal("Unable to retrieve Piece from key: 'I'", exception.Message);
        }

        [Fact]
        public void GetPiece_ShouldThrowException_WhenPieceKeyNotFound()
        {
            // Arrange
            var ply = new Ply { RawMove = "Z4" }; // Invalid character not defined

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => sut.GetPiece(ply));
            Assert.Contains("Unable to retrieve Piece from key:", exception.Message);
        }

        [Fact]
        public void GetPiece_ShouldIdentifyPawnMoveCorrectly()
        {
            // Arrange
            var ply = new Ply { RawMove = "e4" }; // Pawn move
            // Act
            var piece = sut.GetPiece(ply);
            // Assert
            Assert.Equal('P', piece.Name);
            Assert.True(ply.IsPawnMove);
            Assert.True(ply.IsPieceMove);
            Assert.Equal('P', ply.Piece);
        }


        [Fact]
        public void GetPiece_ShouldIdentifyKnightMoveCorrectly()
        {
            // Arrange
            var ply = new Ply { RawMove = "Nf3" }; // Knight move
            // Act
            var piece = sut.GetPiece(ply);
            // Assert
            Assert.Equal('N', piece.Name);
            Assert.False(ply.IsPawnMove);
            Assert.True(ply.IsPieceMove);
            Assert.Equal('N', ply.Piece);
        }

        [Fact]
        public void GetPiece_ShouldIdentifyCaptureMoveCorrectly()
        {
            // Arrange
            var ply = new Ply { RawMove = "Bxe5" }; // Bishop capture move
            // Act
            var piece = sut.GetPiece(ply);
            // Assert
            Assert.Equal('B', piece.Name);
            Assert.True(ply.IsCapture);
            Assert.True(ply.IsPieceMove);
            Assert.Equal('B', ply.Piece);
        }
    }

}
