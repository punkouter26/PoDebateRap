using System.Text.RegularExpressions;

// This file is deprecated. Use PoDebateRap.Shared.Analyzers.RhymeAnalyzer instead.
// Kept for backward compatibility.
namespace PoDebateRap.Shared.Services
{
    /// <summary>
    /// Analyzes rap verses for rhyme patterns, vocabulary richness, and other metrics.
    /// Runs entirely client-side without requiring API calls.
    /// </summary>
    /// <remarks>
    /// **Deprecated**: Use <see cref="Analyzers.RhymeAnalyzer"/> instead.
    /// This class forwards all calls to the new location for backward compatibility.
    /// </remarks>
    [Obsolete("Use PoDebateRap.Shared.Analyzers.RhymeAnalyzer instead")]
    public static class RhymeAnalyzer
    {
        /// <summary>
        /// Analyze a complete verse and return metrics.
        /// </summary>
        public static Models.RapperRhymeMetrics AnalyzeVerse(string verse)
            => Analyzers.RhymeAnalyzer.AnalyzeVerse(verse);

        /// <summary>
        /// Analyze a single turn for crowd reaction purposes.
        /// </summary>
        public static Models.TurnAnalytics AnalyzeTurn(string verse, int turnNumber, bool isRapper1Turn)
            => Analyzers.RhymeAnalyzer.AnalyzeTurn(verse, turnNumber, isRapper1Turn);

        /// <summary>
        /// Combine multiple verses (all turns) for a rapper into final metrics.
        /// </summary>
        public static Models.RapperRhymeMetrics CombineVerses(IEnumerable<string> verses)
            => Analyzers.RhymeAnalyzer.CombineVerses(verses);
    }
}

