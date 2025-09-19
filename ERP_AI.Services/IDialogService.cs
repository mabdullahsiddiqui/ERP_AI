namespace ERP_AI.Services
{
    public interface IDialogService
    {
        void ShowDialog(string dialogKey, object? parameter = null);
        void CloseDialog();
    }
}
