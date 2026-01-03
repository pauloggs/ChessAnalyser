namespace Extensions;


public static class SquareExtensions
{
    public static string Algebraic(this int value)
    {
        var file = (char)('a' + (value % 8));
        var rank = (value / 8) + 1;
        return $"{file}{rank}";
    }
}