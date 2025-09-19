namespace ERP_AI.Services
{
    public interface IFileDialogService
    {
        string? OpenFile(string filter = "*");
        string? SaveFile(string filter = "*");
    }
}
