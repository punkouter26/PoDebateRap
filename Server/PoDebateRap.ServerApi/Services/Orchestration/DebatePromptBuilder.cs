using PoDebateRap.Shared.Models;
using System.Text;

namespace PoDebateRap.ServerApi.Services.Orchestration
{
    /// <summary>
    /// Responsible for building prompts for AI-powered debate generation and judging.
    /// Reduces complexity in DebateOrchestrator by centralizing prompt construction logic.
    /// </summary>
    public interface IDebatePromptBuilder
    {
        /// <summary>
        /// Builds a prompt for generating a debate turn.
        /// </summary>
        string BuildTurnPrompt(
            string currentRapper,
            string opponent,
            string role,
            string topicTitle,
            int currentTurn,
            int totalTurns,
            string? opponentLastVerse);

        /// <summary>
        /// Extracts the opponent's last verse from the debate transcript.
        /// </summary>
        string ExtractOpponentLastVerse(StringBuilder transcript, string opponent, string currentRapper);
    }

    /// <summary>
    /// Default implementation of IDebatePromptBuilder.
    /// </summary>
    public class DebatePromptBuilder : IDebatePromptBuilder
    {
        /// <inheritdoc />
        public string BuildTurnPrompt(
            string currentRapper,
            string opponent,
            string role,
            string topicTitle,
            int currentTurn,
            int totalTurns,
            string? opponentLastVerse)
        {
            if (string.IsNullOrEmpty(opponentLastVerse))
            {
                return BuildOpeningPrompt(currentRapper, role, topicTitle, currentTurn, totalTurns);
            }

            return BuildResponsePrompt(currentRapper, opponent, role, topicTitle, currentTurn, totalTurns, opponentLastVerse);
        }

        /// <inheritdoc />
        public string ExtractOpponentLastVerse(StringBuilder transcript, string opponent, string currentRapper)
        {
            var transcriptLines = transcript.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = transcriptLines.Length - 1; i >= 0; i--)
            {
                if (transcriptLines[i].StartsWith(opponent))
                {
                    var verseBuilder = new StringBuilder();
                    for (int j = i; j < transcriptLines.Length && !transcriptLines[j].StartsWith(currentRapper); j++)
                    {
                        verseBuilder.AppendLine(transcriptLines[j]);
                    }
                    return verseBuilder.ToString().Trim();
                }
            }

            return string.Empty;
        }

        private static string BuildOpeningPrompt(
            string currentRapper,
            string role,
            string topicTitle,
            int currentTurn,
            int totalTurns)
        {
            var positionDescription = role == "Pro" ? "in favor of" : "against";

            return $"You are {currentRapper}, a legendary rapper. You are debating '{topicTitle}'. " +
                   $"Your position is {role} (arguing {positionDescription} the topic). " +
                   $"This is turn {currentTurn} of {totalTurns}. " +
                   $"Open strong with your {role} argument in rap form. Keep it 4-8 bars.";
        }

        private static string BuildResponsePrompt(
            string currentRapper,
            string opponent,
            string role,
            string topicTitle,
            int currentTurn,
            int totalTurns,
            string opponentLastVerse)
        {
            var positionDescription = role == "Pro" ? "in favor of" : "against";

            return $"You are {currentRapper}, a legendary rapper debating {opponent} on '{topicTitle}'. " +
                   $"Your position is {role} (arguing {positionDescription} the topic). " +
                   $"This is turn {currentTurn} of {totalTurns}. " +
                   $"{opponent} just said:\n{opponentLastVerse}\n\n" +
                   $"Respond DIRECTLY to their points. Counter their arguments, call out their weak bars, and strengthen your {role} position. " +
                   $"Make it personal and reference what they just said. Keep it 4-8 bars.";
        }
    }
}
