using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.ViewModels;

internal sealed class TestDialogs : IDialogManager
{
    public MemoryStream Output { get; private set; }
    public bool Cancel { get; set; }
    public bool Fail { get; set; }
    public TaskCompletionSource<bool> Wait { get; set; }
    public int Saves { get; private set; }
    public async Task<(string filename, Stream stream)> SaveFileDialog(string title, string directory = "", string initialFileName = "", string defaultExtension = "", FilePickerFileType[] filters = null)
    {
        Saves++;
        if (Wait != null) await Wait.Task;
        if (Cancel) return (null, null);
        if (Fail) throw new IOException("Falha simulada de escrita");
        Output = new MemoryStream();
        Output.Write(new byte[200000]); // Simulate replacing a longer existing file.
        return (initialFileName, Output);
    }
    public void CloseApp() => throw new NotSupportedException();
    public void UpdateVersionInfo(string version) => throw new NotSupportedException();
    public void SetTitle(string title) => throw new NotSupportedException();
    public void SetFileName(string file) => throw new NotSupportedException();
    public void AddMessage(string message) => throw new NotSupportedException();
    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Information, TimeSpan? expiration = null, Action onClick = null, Action onClose = null) => throw new NotSupportedException();
    public void ShowChildDialog<V>(V vm) where V : ViewModelBase => throw new NotSupportedException();
    public Task ShowChildDialogAsync<V>(V vm) where V : ViewModelBase => throw new NotSupportedException();
    public Task<bool> ShowMessageDialogAsync(string title, string message, MessageDialogType messageDialogType) => throw new NotSupportedException();
    public void CloseDialog(bool result = false) => throw new NotSupportedException();
    public Task<string[]> OpenFileDialog(string title, string initialFileName = "", bool allowMultiple = false, FilePickerFileType[] filters = null) => throw new NotSupportedException();
    public Task<string> OpenFolderDialog(string title, string directory = "") => throw new NotSupportedException();
}
