namespace ERP_AI.Services
{
    public interface IProgressService
    {
        void ShowProgress(string message = "");
        void HideProgress();
        void Report(double value);
    }
}
