using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Services
{
    public interface IDialogManager
    {
        void CloseApp();
        void UpdateVersionInfo(string version);
        void SetTitle(string title);
        void SetFileName(string file);
        void AddMessage(string message);
        void ShowNotification(string title, string message, NotificationType type = NotificationType.Information, TimeSpan? expiration = null, Action onClick = null, Action onClose = null);
        void ShowChildDialog(Window window);
        Task ShowChildDialogAsync(Window window);
        Task<T> ShowChildDialogAsync<T>(Window window);
        void CloseDialog(bool result = false);
        Task<string[]> OpenFileDialog(string title, string directory = "", string initialFileName = "", bool allowMultiple = false, List<FileDialogFilter> filters = null);
        Task<string> OpenFolderDialog(string title, string directory = "");
        Task<string> SaveFileDialog(string title, string directory = "", string initialFileName = "", string defaultExtension = "", List<FileDialogFilter> filters = null);
    }
}
