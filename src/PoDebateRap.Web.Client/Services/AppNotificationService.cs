using Radzen;

namespace PoDebateRap.Web.Client.Services;

/// <summary>
/// Wrapper service for Radzen NotificationService with application-specific helpers.
/// </summary>
public class AppNotificationService
{
    private readonly NotificationService _notificationService;

    public AppNotificationService(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void ShowSuccess(string message, string title = "Success")
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = title,
            Detail = message,
            Duration = 3000
        });
    }

    public void ShowError(string message, string title = "Error")
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = title,
            Detail = message,
            Duration = 5000
        });
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = title,
            Detail = message,
            Duration = 4000
        });
    }

    public void ShowInfo(string message, string title = "Info")
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = title,
            Detail = message,
            Duration = 3000
        });
    }
}
