namespace ERP_AI.Services
{
    public interface IMessageBoxService
    {
        void ShowMessage(string message, string title = "", MessageBoxType type = MessageBoxType.Info);
    }

    public enum MessageBoxType
    {
        Info,
        Warning,
        Error,
        Success
    }
}
