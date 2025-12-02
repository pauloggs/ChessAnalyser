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
            var plyPawn = new Interfaces.DTO.Ply { RawMove = "e4" };
            var plyKnight = new Interfaces.DTO.Ply { RawMove = "Nf3" };
            var plyCapture = new Interfaces.DTO.Ply { RawMove = "Bxe5" };
            var plyPromotion = new Interfaces.DTO.Ply { RawMove = "e8=Q" };
            var plyKingsideCastling = new Interfaces.DTO.Ply { RawMove = "O-O" };
            var plyQueensideCastling = new Interfaces.DTO.Ply { RawMove = "O-O-O" };

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
    }
}
