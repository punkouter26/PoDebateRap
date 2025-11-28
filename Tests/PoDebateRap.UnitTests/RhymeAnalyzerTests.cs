using FluentAssertions;
using PoDebateRap.Shared.Models;
using PoDebateRap.Shared.Services;
using Xunit;

namespace PoDebateRap.UnitTests
{
    public class RhymeAnalyzerTests
    {
        [Fact]
        public void AnalyzeVerse_WithEmptyText_ReturnsEmptyMetrics()
        {
            // Act
            var result = RhymeAnalyzer.AnalyzeVerse("");

            // Assert
            result.Should().NotBeNull();
            result.TotalWords.Should().Be(0);
            result.UniqueWords.Should().Be(0);
            result.TotalLines.Should().Be(0);
        }

        [Fact]
        public void AnalyzeVerse_WithSimpleVerse_CountsWordsCorrectly()
        {
            // Arrange
            var verse = @"I got the flow like water
Making rappers scatter
My words hit like thunder
Putting competition under";

            // Act
            var result = RhymeAnalyzer.AnalyzeVerse(verse);

            // Assert
            result.TotalWords.Should().BeGreaterThan(10);
            result.TotalLines.Should().Be(4);
            result.UniqueWords.Should().BeGreaterThan(0);
        }

        [Fact]
        public void AnalyzeVerse_WithRhymingLines_DetectsRhymes()
        {
            // Arrange - AA rhyme scheme (water/scatter, thunder/under)
            var verse = @"I got the flow like water
Making rappers scatter
My words hit like thunder
Putting competition under";

            // Act
            var result = RhymeAnalyzer.AnalyzeVerse(verse);

            // Assert
            result.RhymeDensity.Should().BeGreaterThan(0, "verse contains rhyming endings");
            result.RhymeCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void AnalyzeVerse_WithAlliteration_DetectsAlliteration()
        {
            // Arrange - heavy alliteration (big, boss, bringing, bars)
            var verse = @"Big boss bringing bars
Cold cash counting cars
Sick style spitting straight
Fire flow feeling great";

            // Act
            var result = RhymeAnalyzer.AnalyzeVerse(verse);

            // Assert
            result.AlliterationScore.Should().BeGreaterThan(0, "verse contains alliteration");
        }

        [Fact]
        public void AnalyzeVerse_CalculatesVocabularyRichness()
        {
            // Arrange - diverse vocabulary
            var verse = @"Magnificent eloquent sophisticated bars
Destroying obliterating eliminating stars
Intellectual philosophical metaphysical flow
Phenomenal astronomical making the show";

            // Act
            var result = RhymeAnalyzer.AnalyzeVerse(verse);

            // Assert
            result.VocabularyRichness.Should().BeGreaterThan(50, "all words are unique");
            result.WordComplexity.Should().BeGreaterThan(30, "words are long/complex");
        }

        [Fact]
        public void AnalyzeTurn_ReturnsCorrectReactionType()
        {
            // Arrange - strong verse with rhymes
            var verse = @"I dominate the microphone with lyrical precision
Every bar I spit is surgical, making the incision
My flow is legendary, they study what I say
I'm the king of this arena, bow down and obey";

            // Act
            var result = RhymeAnalyzer.AnalyzeTurn(verse, 1, true);

            // Assert
            result.TurnNumber.Should().Be(1);
            result.IsRapper1Turn.Should().BeTrue();
            result.OverallScore.Should().BeGreaterThan(0);
            result.GetReactionType().Should().NotBe(CrowdReactionType.None);
        }

        [Fact]
        public void CombineVerses_CombinesMultipleVerses()
        {
            // Arrange
            var verses = new[]
            {
                "First verse spitting fire today",
                "Second verse bringing the heat okay",
                "Third verse ending the show my way"
            };

            // Act
            var result = RhymeAnalyzer.CombineVerses(verses);

            // Assert
            result.TotalLines.Should().Be(3);
            result.TotalWords.Should().BeGreaterThan(10);
        }

        [Theory]
        [InlineData(85, CrowdReactionType.Fireworks)]
        [InlineData(70, CrowdReactionType.Confetti)]
        [InlineData(50, CrowdReactionType.Cheers)]
        [InlineData(20, CrowdReactionType.ThumbsDown)]
        public void TurnAnalytics_GetReactionType_ReturnsCorrectType(double score, CrowdReactionType expectedType)
        {
            // Arrange
            var analytics = new TurnAnalytics
            {
                TurnNumber = 1,
                IsRapper1Turn = true,
                OverallScore = score
            };

            // Act
            var result = analytics.GetReactionType();

            // Assert
            result.Should().Be(expectedType);
        }
    }
}
