namespace ERP_AI.Services
{
    public interface INotificationService
    {
        void ShowNotification(string message, NotificationType type = NotificationType.Info);
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}
