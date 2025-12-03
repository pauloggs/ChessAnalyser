namespace Services.Helpers
{
    public static class RankAndFileHelper
    {
        /// <summary>
        /// Check if the potential rank or file matches the specified rank or file.
        /// </summary>
        /// <param name="potentialRank"></param>
        /// <param name="potentialFile"></param>
        /// <param name="specifiedRank"></param>
        /// <param name="specifiedFile"></param>
        /// <returns></returns>
        public static bool PotentialRankOrFileMatchesSpecifiedRankOrFile(
            int potentialRank,
            int potentialFile,
            int specifiedRank,
            int specifiedFile)
        {
            // if neither specified, always matches
            if (specifiedRank < 0 && specifiedFile < 0) return true; // matches if nothing specified

            // if one matches, then it's a match
            if (potentialRank == specifiedRank
                || potentialFile == specifiedFile) return true;

            // else no match
            return false;
        }
    }
}
