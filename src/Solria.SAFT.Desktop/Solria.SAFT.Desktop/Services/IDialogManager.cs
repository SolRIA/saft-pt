using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.ViewModels;
using System;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.Services;

public interface IDialogManager
{
    void CloseApp();
    void UpdateVersionInfo(string version);
    void SetTitle(string title);
    void SetFileName(string file);
    void AddMessage(string message);
    void ShowNotification(string title, string message, NotificationType type = NotificationType.Information, TimeSpan? expiration = null, Action onClick = null, Action onClose = null);
    void ShowChildDialog<V>(V vm) where V : ViewModelBase;
    Task ShowChildDialogAsync<V>(V vm) where V : ViewModelBase;
    Task<bool> ShowMessageDialogAsync(string title, string message, MessageDialogType messageDialogType);
    void CloseDialog(bool result = false);
    Task<string[]> OpenFileDialog(string title, string initialFileName = "", bool allowMultiple = false, FilePickerFileType[] filters = null);
    Task<string> SaveFileDialog(string title, string directory = "", string initialFileName = "", string defaultExtension = "", FilePickerFileType[] filters = null);
    Task<string> OpenFolderDialog(string title, string directory = "");
}
