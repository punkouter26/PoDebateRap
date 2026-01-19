using System.Text.RegularExpressions;

namespace PoDebateRap.Shared.Analyzers
{
    /// <summary>
    /// Analyzes rap verses for rhyme patterns, vocabulary richness, and other metrics.
    /// Runs entirely client-side without requiring API calls.
    /// </summary>
    public static class RhymeAnalyzer
    {
        // Common vowel sounds for rhyme detection
        private static readonly Dictionary<string, string> VowelPatterns = new()
        {
            { "ay", "A" }, { "ai", "A" }, { "a", "A" },
            { "ee", "E" }, { "ea", "E" }, { "ie", "E" }, { "y", "E" },
            { "igh", "I" }, { "i", "I" }, { "eye", "I" },
            { "ow", "O" }, { "oa", "O" }, { "o", "O" },
            { "oo", "U" }, { "ue", "U" }, { "ew", "U" }, { "u", "U" }
        };

        /// <summary>
        /// Analyze a complete verse and return metrics.
        /// </summary>
        public static Models.RapperRhymeMetrics AnalyzeVerse(string verse)
        {
            if (string.IsNullOrWhiteSpace(verse))
            {
                return new Models.RapperRhymeMetrics();
            }

            var lines = verse.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(l => l.Trim())
                             .Where(l => !string.IsNullOrEmpty(l))
                             .ToArray();

            var words = ExtractWords(verse);
            var uniqueWords = words.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var rhymeCount = CountRhymes(lines);
            var alliterationCount = CountAlliteration(lines);
            var syllableCounts = words.Select(CountSyllables).ToList();
            var avgSyllables = syllableCounts.Count > 0 ? syllableCounts.Average() : 0;
            var avgWordLength = words.Count > 0 ? words.Average(w => w.Length) : 0;

            // Calculate normalized scores (0-100)
            var rhymeDensity = lines.Length > 0 ? Math.Min(100, (rhymeCount / (double)lines.Length) * 50) : 0;
            var vocabRichness = words.Count > 0 ? (uniqueWords.Count / (double)words.Count) * 100 : 0;
            var syllableComplexity = Math.Min(100, (avgSyllables / 4.0) * 100); // 4 syllables = max
            var wordComplexity = Math.Min(100, (avgWordLength / 10.0) * 100); // 10 chars = max
            var alliterationScore = lines.Length > 0 ? Math.Min(100, (alliterationCount / (double)lines.Length) * 100) : 0;

            return new Models.RapperRhymeMetrics
            {
                RhymeDensity = Math.Round(rhymeDensity, 1),
                VocabularyRichness = Math.Round(vocabRichness, 1),
                SyllableComplexity = Math.Round(syllableComplexity, 1),
                WordComplexity = Math.Round(wordComplexity, 1),
                AlliterationScore = Math.Round(alliterationScore, 1),
                TotalWords = words.Count,
                UniqueWords = uniqueWords.Count,
                TotalLines = lines.Length,
                RhymeCount = rhymeCount,
                AverageWordLength = Math.Round(avgWordLength, 2),
                AverageSyllables = Math.Round(avgSyllables, 2)
            };
        }

        /// <summary>
        /// Analyze a single turn for crowd reaction purposes.
        /// </summary>
        public static Models.TurnAnalytics AnalyzeTurn(string verse, int turnNumber, bool isRapper1Turn)
        {
            var metrics = AnalyzeVerse(verse);

            // Calculate overall score as weighted average
            var overallScore = (
                metrics.RhymeDensity * 0.30 +
                metrics.VocabularyRichness * 0.25 +
                metrics.SyllableComplexity * 0.15 +
                metrics.WordComplexity * 0.15 +
                metrics.AlliterationScore * 0.15
            );

            return new Models.TurnAnalytics
            {
                TurnNumber = turnNumber,
                IsRapper1Turn = isRapper1Turn,
                OverallScore = Math.Round(overallScore, 1),
                RhymeDensity = metrics.RhymeDensity,
                VocabularyRichness = metrics.VocabularyRichness,
                WordCount = metrics.TotalWords
            };
        }

        /// <summary>
        /// Combine multiple verses (all turns) for a rapper into final metrics.
        /// </summary>
        public static Models.RapperRhymeMetrics CombineVerses(IEnumerable<string> verses)
        {
            var combined = string.Join("\n", verses);
            return AnalyzeVerse(combined);
        }

        #region Private Helper Methods

        private static List<string> ExtractWords(string text)
        {
            return Regex.Matches(text, @"\b[a-zA-Z]+\b")
                        .Select(m => m.Value.ToLowerInvariant())
                        .ToList();
        }

        private static int CountRhymes(string[] lines)
        {
            if (lines.Length < 2) return 0;

            int rhymeCount = 0;
            var lineEndings = lines.Select(GetLineEnding).ToList();

            // Check adjacent lines (AA pattern)
            for (int i = 0; i < lineEndings.Count - 1; i++)
            {
                if (DoWordsRhyme(lineEndings[i], lineEndings[i + 1]))
                {
                    rhymeCount++;
                }
            }

            // Check alternating lines (ABAB pattern)
            for (int i = 0; i < lineEndings.Count - 2; i += 2)
            {
                if (i + 2 < lineEndings.Count && DoWordsRhyme(lineEndings[i], lineEndings[i + 2]))
                {
                    rhymeCount++;
                }
            }

            return rhymeCount;
        }

        private static string GetLineEnding(string line)
        {
            var words = ExtractWords(line);
            return words.Count > 0 ? words[^1] : "";
        }

        private static bool DoWordsRhyme(string word1, string word2)
        {
            if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2))
                return false;

            if (word1.Equals(word2, StringComparison.OrdinalIgnoreCase))
                return false; // Same word doesn't count as rhyme

            // Check ending sounds
            var ending1 = GetPhoneticEnding(word1);
            var ending2 = GetPhoneticEnding(word2);

            return ending1.Length >= 2 && ending1.Equals(ending2, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetPhoneticEnding(string word)
        {
            if (word.Length < 2) return word;

            // Get last 2-3 characters as simple phonetic approximation
            int takeLength = Math.Min(3, word.Length);
            return word[^takeLength..];
        }

        private static int CountAlliteration(string[] lines)
        {
            int count = 0;

            foreach (var line in lines)
            {
                var words = ExtractWords(line).Where(w => w.Length > 2).ToList();
                if (words.Count < 2) continue;

                // Count consecutive words starting with same consonant
                for (int i = 0; i < words.Count - 1; i++)
                {
                    char c1 = words[i][0];
                    char c2 = words[i + 1][0];

                    if (c1 == c2 && !IsVowel(c1))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static bool IsVowel(char c)
        {
            return "aeiou".Contains(char.ToLower(c));
        }

        private static int CountSyllables(string word)
        {
            if (string.IsNullOrEmpty(word)) return 0;

            word = word.ToLowerInvariant();
            int count = 0;
            bool lastWasVowel = false;

            foreach (char c in word)
            {
                bool isVowel = IsVowel(c);
                if (isVowel && !lastWasVowel)
                {
                    count++;
                }
                lastWasVowel = isVowel;
            }

            // Adjust for silent e
            if (word.EndsWith("e") && count > 1)
            {
                count--;
            }

            // Ensure at least 1 syllable
            return Math.Max(1, count);
        }

        #endregion
    }
}
