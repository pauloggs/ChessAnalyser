namespace Services.Helpers
{
    /// <summary>
    /// Heuristic to treat two forenames as the same player: exact match, or one is an abbreviation of the other
    /// (e.g. "Joseph H" vs "Joseph Henry", "Henry E" vs "Henry Ernst"). Also normalizes "E/Durrfel" to "E" so
    /// variants with different suffixes after "/" are treated as the same.
    /// </summary>
    public static class PlayerForenamesMatcher
    {
        private static readonly StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Primary forenames for matching: part before "/" if present (e.g. "E/Durrfel" → "E"), trimmed.
        /// </summary>
        public static string NormalizeForMatching(string? forenames)
        {
            var s = (forenames ?? string.Empty).Trim();
            var slash = s.IndexOf('/');
            if (slash >= 0)
                s = s[..slash].Trim();
            return s;
        }

        /// <summary>
        /// True if the two forenames represent the same person: equal after normalizing, or one is a
        /// token-by-token abbreviation of the other (e.g. "Joseph H" vs "Joseph Henry").
        /// </summary>
        public static bool ForenamesMatch(string? forenames1, string? forenames2)
        {
            var a = NormalizeForMatching(forenames1);
            var b = NormalizeForMatching(forenames2);
            if (string.Equals(a, b, Comparison))
                return true;
            var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokensA.Length == 0 && tokensB.Length == 0)
                return true;
            if (tokensA.Length == 0 || tokensB.Length == 0)
                return false;
            // For each position up to the shorter sequence length, the longer token must start with the shorter
            // (e.g. "A" vs "Alexander", "Alexander" vs "Alexander A", "Joseph H" vs "Joseph Henry").
            var len = Math.Min(tokensA.Length, tokensB.Length);
            for (var i = 0; i < len; i++)
            {
                var t1 = tokensA[i];
                var t2 = tokensB[i];
                if (t1.Length <= t2.Length)
                {
                    if (!t2.StartsWith(t1, Comparison))
                        return false;
                }
                else
                {
                    if (!t1.StartsWith(t2, Comparison))
                        return false;
                }
            }
            return true;
        }
    }
}
