using Radzen;

namespace PoDebateRap.Client.Services;

/// <summary>
/// Wrapper service for Radzen NotificationService with app-specific styling
/// </summary>
public class AppNotificationService
{
    private readonly NotificationService _notificationService;

    public AppNotificationService(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void ShowSuccess(string message, string? summary = null, double duration = 4000)
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = summary ?? "Success",
            Detail = message,
            Duration = duration,
            Style = "background: var(--bg-elevated); border-left: 4px solid var(--accent-green);"
        });
    }

    public void ShowError(string message, string? summary = null, double duration = 6000)
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = summary ?? "Error",
            Detail = message,
            Duration = duration,
            Style = "background: var(--bg-elevated); border-left: 4px solid var(--accent-red);"
        });
    }

    public void ShowWarning(string message, string? summary = null, double duration = 5000)
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = summary ?? "Warning",
            Detail = message,
            Duration = duration,
            Style = "background: var(--bg-elevated); border-left: 4px solid var(--accent-gold);"
        });
    }

    public void ShowInfo(string message, string? summary = null, double duration = 4000)
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = summary ?? "Info",
            Detail = message,
            Duration = duration,
            Style = "background: var(--bg-elevated); border-left: 4px solid #2196F3;"
        });
    }

    public void ShowDebateStarted(string rapper1, string rapper2)
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "⚔️ Battle Started!",
            Detail = $"{rapper1} vs {rapper2} - Let the rap battle begin!",
            Duration = 3000
        });
    }

    public void ShowDebateComplete(string? winner)
    {
        var message = string.IsNullOrEmpty(winner) || winner is "Draw" or "Undecided"
            ? "The battle has ended!"
            : $"🏆 {winner} wins the battle!";

        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "🎤 Battle Complete!",
            Detail = message,
            Duration = 5000
        });
    }

    public void ShowConnectionError()
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Connection Error",
            Detail = "Unable to connect to the server. Please check your connection.",
            Duration = 8000
        });
    }
}
