using Services.Helpers;

namespace ServicesTests.Helpers
{
    public class PlayerForenamesMatcherTests
    {
        [Fact]
        public void ForenamesMatch_JosephH_And_JosephHenry_ReturnsTrue()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Joseph H", "Joseph Henry"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Joseph Henry", "Joseph H"));
        }

        [Fact]
        public void ForenamesMatch_HenryE_And_HenryErnst_ReturnsTrue()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Henry E", "Henry Ernst"));
        }

        [Fact]
        public void ForenamesMatch_WilliamA_And_WilliamAlbert_ReturnsTrue()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("William A", "William Albert"));
        }

        [Fact]
        public void ForenamesMatch_AdolfG_And_AdolfGeorg_ReturnsTrue()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Adolf G", "Adolf Georg"));
        }

        [Fact]
        public void ForenamesMatch_EDurrfel_And_EPfaffen_ReturnsTrue()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("E/Durrfel", "E/Pfaffen"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("E/Stolzyk", "E/X"));
        }

        [Fact]
        public void NormalizeForMatching_TakesPartBeforeSlash()
        {
            Assert.Equal("E", PlayerForenamesMatcher.NormalizeForMatching("E/Durrfel"));
            Assert.Equal("E", PlayerForenamesMatcher.NormalizeForMatching("E/Pfaffen"));
        }

        [Fact]
        public void ForenamesMatch_ExactMatch_ReturnsTrue()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Joseph Henry", "Joseph Henry"));
        }

        [Fact]
        public void ForenamesMatch_DifferentSurnames_NotChecked_OnlyForenames()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Alexander A", "Alexander A"));
        }

        [Fact]
        public void ForenamesMatch_UnrelatedForenames_ReturnsFalse()
        {
            Assert.False(PlayerForenamesMatcher.ForenamesMatch("Joseph", "Henry"));
            Assert.False(PlayerForenamesMatcher.ForenamesMatch("William A", "William B"));
        }

        [Fact]
        public void ForenamesMatch_AlekhineVariants_AllMatch()
        {
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("A", "Alexander"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Alexander", "A"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("A", "Alexander A"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Alexander A", "A"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Alexander", "Alexander A"));
            Assert.True(PlayerForenamesMatcher.ForenamesMatch("Alexander A", "Alexander"));
        }
    }
}
