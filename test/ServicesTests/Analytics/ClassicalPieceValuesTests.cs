using Interfaces.Analytics;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class ClassicalPieceValuesTests
{
    private readonly IPieceValues _sut = new ClassicalPieceValues();

    [Theory]
    [InlineData('P', 1)]
    [InlineData('p', 1)]
    [InlineData('N', 3)]
    [InlineData('B', 3)]
    [InlineData('R', 5)]
    [InlineData('Q', 9)]
    [InlineData('K', 0)]
    public void ValueForPieceType_ReturnsClassicalValues(char piece, short expected) =>
        Assert.Equal(expected, _sut.ValueForPieceType(piece));

    [Fact]
    public void ValueForPieceType_Invalid_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.ValueForPieceType('X'));
}
