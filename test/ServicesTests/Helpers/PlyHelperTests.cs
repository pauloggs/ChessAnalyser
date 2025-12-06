using Interfaces.DTO;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests.Helpers
{
    public class PlyHelperTests
    {
        [Fact]
        public void AddPlies_ShouldAddPliesCorrectly()
        {
            // Arrange
            var plyDictionary = new Dictionary<int, Ply>();
            string line = "1. e4 e5 2. Nf3 Nc6 3. Bb5 a6";
            int plyNumber = 0;
            // Act
            PlyHelper.AddPlies(plyDictionary, line, ref plyNumber);
            // Assert
            Assert.Equal(6, plyDictionary.Count);
            Assert.Equal("e4", plyDictionary[0].RawMove);
            Assert.Equal(Colour.W, plyDictionary[0].Colour);
            Assert.Equal(1, plyDictionary[0].MoveNumber);
            Assert.Equal("e5", plyDictionary[1].RawMove);
            Assert.Equal(Colour.B, plyDictionary[1].Colour);
            Assert.Equal(1, plyDictionary[1].MoveNumber);
            Assert.Equal("Nf3", plyDictionary[2].RawMove);
            Assert.Equal(Colour.W, plyDictionary[2].Colour);
            Assert.Equal(2, plyDictionary[2].MoveNumber);
            Assert.Equal("Nc6", plyDictionary[3].RawMove);
            Assert.Equal(Colour.B, plyDictionary[3].Colour);
            Assert.Equal(2, plyDictionary[3].MoveNumber);
            Assert.Equal("Bb5", plyDictionary[4].RawMove);
            Assert.Equal(Colour.W, plyDictionary[4].Colour);
            Assert.Equal(3, plyDictionary[4].MoveNumber);
            Assert.Equal("a6", plyDictionary[5].RawMove);
            Assert.Equal(Colour.B, plyDictionary[5].Colour);
            Assert.Equal(3, plyDictionary[5].MoveNumber);
        }

        [Fact]
        public void AddPlies_ShouldHandleEmptyLines()
        {
            // Arrange
            var plyDictionary = new Dictionary<int, Ply>();
            string line = "   ";
            int plyNumber = 0;
            // Act
            PlyHelper.AddPlies(plyDictionary, line, ref plyNumber);
            // Assert
            Assert.Empty(plyDictionary);
            Assert.Equal(0, plyNumber);
        }

        [Fact]
        public void AddPlies_ShouldIgnoreMoveNumbers()
        {
            // Arrange
            var plyDictionary = new Dictionary<int, Ply>();
            string line = "1. e4 2. e5 3. Nf3";
            int plyNumber = 0;
            // Act
            PlyHelper.AddPlies(plyDictionary, line, ref plyNumber);
            // Assert
            Assert.Equal(3, plyDictionary.Count);
            Assert.Equal("e4", plyDictionary[0].RawMove);
            Assert.Equal("e5", plyDictionary[1].RawMove);
            Assert.Equal("Nf3", plyDictionary[2].RawMove);
        }
    }
}
