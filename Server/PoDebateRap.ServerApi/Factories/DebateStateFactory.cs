using PoDebateRap.Shared.Models;
using System.Text;

namespace PoDebateRap.ServerApi.Factories;

/// <summary>
/// Factory for creating DebateState instances with consistent initialization.
/// Eliminates duplicate DebateState initialization logic.
/// </summary>
public static class DebateStateFactory
{
    /// <summary>
    /// Creates an empty DebateState with all properties properly initialized.
    /// </summary>
    public static DebateState CreateEmpty()
    {
        return new DebateState
        {
            Rapper1 = new Rapper(),
            Rapper2 = new Rapper(),
            Topic = new Topic(),
            CurrentTurnAudio = [],
            WinnerName = string.Empty,
            JudgeReasoning = string.Empty,
            Stats = new DebateStats(),
            ErrorMessage = string.Empty,
            IsDebateInProgress = false,
            IsGeneratingTurn = false,
            IsDebateFinished = false,
            CurrentTurn = 0,
            TotalTurns = 0,
            CurrentTurnText = string.Empty,
            IsRapper1Turn = true
        };
    }

    /// <summary>
    /// Creates an initialized DebateState for a new debate.
    /// </summary>
    public static DebateState CreateForNewDebate(
        Rapper rapper1,
        Rapper rapper2,
        Topic topic,
        int totalTurns = 10)
    {
        return new DebateState
        {
            Rapper1 = rapper1,
            Rapper2 = rapper2,
            Topic = topic,
            IsDebateInProgress = true,
            CurrentTurn = 0,
            TotalTurns = totalTurns,
            DebateTranscript = new StringBuilder(),
            IsRapper1Turn = true,
            CurrentTurnText = $"Get ready! Topic: '{topic.Title}'. {rapper1.Name} (Pro) vs {rapper2.Name} (Con). {rapper1.Name} starts...",
            IsGeneratingTurn = false,
            CurrentTurnAudio = [],
            WinnerName = string.Empty,
            JudgeReasoning = string.Empty,
            Stats = new DebateStats(),
            ErrorMessage = string.Empty
        };
    }
}
