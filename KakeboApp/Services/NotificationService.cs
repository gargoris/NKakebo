// Services/NotificationService.cs - Sistema de notificaciones

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Threading;

namespace KakeboApp.Services;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public class NotificationMessage : ReactiveObject
{
    private bool _isVisible = true;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public TimeSpan AutoHideAfter { get; set; } = TimeSpan.FromSeconds(5);

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public string TypeIcon => Type switch
    {
        NotificationType.Info => "ℹ️",
        NotificationType.Success => "✅",
        NotificationType.Warning => "⚠️",
        NotificationType.Error => "❌",
        _ => "ℹ️"
    };

    public string TypeColor => Type switch
    {
        NotificationType.Info => "#2196F3",
        NotificationType.Success => "#4CAF50",
        NotificationType.Warning => "#FF9800",
        NotificationType.Error => "#F44336",
        _ => "#2196F3"
    };
}

public interface INotificationService
{
    ObservableCollection<NotificationMessage> Notifications { get; }
    
    void ShowInfo(string title, string message, TimeSpan? autoHideAfter = null);
    void ShowSuccess(string title, string message, TimeSpan? autoHideAfter = null);
    void ShowWarning(string title, string message, TimeSpan? autoHideAfter = null);
    void ShowError(string title, string message, TimeSpan? autoHideAfter = null);
    void ShowException(Exception exception, string? title = null);
    
    void Hide(NotificationMessage notification);
    void ClearAll();
}

public class NotificationService : ReactiveObject, INotificationService
{
    public ObservableCollection<NotificationMessage> Notifications { get; } = new();

    public void ShowInfo(string title, string message, TimeSpan? autoHideAfter = null)
    {
        Show(NotificationType.Info, title, message, autoHideAfter);
    }

    public void ShowSuccess(string title, string message, TimeSpan? autoHideAfter = null)
    {
        Show(NotificationType.Success, title, message, autoHideAfter);
    }

    public void ShowWarning(string title, string message, TimeSpan? autoHideAfter = null)
    {
        Show(NotificationType.Warning, title, message, autoHideAfter);
    }

    public void ShowError(string title, string message, TimeSpan? autoHideAfter = null)
    {
        Show(NotificationType.Error, title, message, autoHideAfter ?? TimeSpan.FromSeconds(10));
    }

    public void ShowException(Exception exception, string? title = null)
    {
        ShowError(
            title ?? "Error",
            exception.Message,
            TimeSpan.FromSeconds(10)
        );
    }

    private void Show(NotificationType type, string title, string message, TimeSpan? autoHideAfter)
    {
        var notification = new NotificationMessage
        {
            Type = type,
            Title = title,
            Message = message,
            AutoHideAfter = autoHideAfter ?? TimeSpan.FromSeconds(5)
        };

        Dispatcher.UIThread.Post(() =>
        {
            Notifications.Add(notification);

            if (notification.AutoHideAfter > TimeSpan.Zero)
            {
                Task.Delay(notification.AutoHideAfter).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() => Hide(notification));
                });
            }
        });
    }

    public void Hide(NotificationMessage notification)
    {
        Dispatcher.UIThread.Post(() =>
        {
            notification.IsVisible = false;
            Task.Delay(300).ContinueWith(_ => // Delay para animación
            {
                Dispatcher.UIThread.Post(() => Notifications.Remove(notification));
            });
        });
    }

    public void ClearAll()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var notification in Notifications)
            {
                notification.IsVisible = false;
            }
            Task.Delay(300).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() => Notifications.Clear());
            });
        });
    }
}
