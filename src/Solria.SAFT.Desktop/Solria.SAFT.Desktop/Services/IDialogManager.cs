using Avalonia.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Services
{
    public interface IDialogManager
    {
        void ShowChildDialog(Window window);
        Task ShowChildDialogAsync(Window window);
        Task ShowChildDialogAsync<T>(Window window);
        Task<string[]> OpenFileDialog(string title, string directory = "", string initialFileName = "", bool allowMultiple = false, List<FileDialogFilter> filters = null);
        Task<string> OpenFolderDialog(string title, string directory = "");
        Task<string> SaveFileDialog(string title, string directory = "", string initialFileName = "", string defaultExtension = "", List<FileDialogFilter> filters = null);
    }
}
