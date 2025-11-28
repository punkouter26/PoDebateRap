namespace PoDebateRap.Shared.Models
{
    /// <summary>
    /// Analytics metrics for a single rapper's performance in a debate.
    /// All scores are normalized to 0-100 scale for spider chart visualization.
    /// </summary>
    public class RapperRhymeMetrics
    {
        /// <summary>
        /// Rhymes per bar/line (normalized 0-100).
        /// Higher = more rhyming content.
        /// </summary>
        public double RhymeDensity { get; set; }

        /// <summary>
        /// Unique words / Total words ratio (normalized 0-100).
        /// Higher = more varied vocabulary.
        /// </summary>
        public double VocabularyRichness { get; set; }

        /// <summary>
        /// Average syllables per word (normalized 0-100).
        /// Higher = more complex word choices.
        /// </summary>
        public double SyllableComplexity { get; set; }

        /// <summary>
        /// Average word length in characters (normalized 0-100).
        /// Higher = longer, more sophisticated words.
        /// </summary>
        public double WordComplexity { get; set; }

        /// <summary>
        /// Repeated consonant sounds at word starts (normalized 0-100).
        /// Higher = more poetic alliteration.
        /// </summary>
        public double AlliterationScore { get; set; }

        // Raw values for display
        public int TotalWords { get; set; }
        public int UniqueWords { get; set; }
        public int TotalLines { get; set; }
        public int RhymeCount { get; set; }
        public double AverageWordLength { get; set; }
        public double AverageSyllables { get; set; }
    }

    /// <summary>
    /// Combined analytics for both rappers in a debate.
    /// </summary>
    public class RhymeAnalytics
    {
        public required RapperRhymeMetrics Rapper1Metrics { get; set; }
        public required RapperRhymeMetrics Rapper2Metrics { get; set; }

        /// <summary>
        /// Per-turn analytics for tracking progression.
        /// </summary>
        public List<TurnAnalytics> TurnByTurnAnalytics { get; set; } = new();

        public static RhymeAnalytics Empty => new()
        {
            Rapper1Metrics = new RapperRhymeMetrics(),
            Rapper2Metrics = new RapperRhymeMetrics()
        };
    }

    /// <summary>
    /// Analytics for a single turn, used for crowd reactions.
    /// </summary>
    public class TurnAnalytics
    {
        public int TurnNumber { get; set; }
        public bool IsRapper1Turn { get; set; }
        public double OverallScore { get; set; } // 0-100, used to determine reaction type
        public double RhymeDensity { get; set; }
        public double VocabularyRichness { get; set; }
        public int WordCount { get; set; }

        /// <summary>
        /// Determines the crowd reaction type based on overall score.
        /// </summary>
        public CrowdReactionType GetReactionType()
        {
            return OverallScore switch
            {
                >= 80 => CrowdReactionType.Fireworks,  // Exceptional verse
                >= 60 => CrowdReactionType.Confetti,   // Good verse
                >= 40 => CrowdReactionType.Cheers,     // Average verse
                _ => CrowdReactionType.ThumbsDown      // Below average
            };
        }
    }

    /// <summary>
    /// Types of crowd reactions that can be triggered.
    /// </summary>
    public enum CrowdReactionType
    {
        None,
        Cheers,      // Mild positive (clapping)
        Confetti,    // Good (colorful confetti)
        Fireworks,   // Excellent (full fireworks)
        ThumbsDown   // Poor (thumbs down)
    }
}
