using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class RecentFileItemViewModel : ObservableObject
{
    public RecentFileItemViewModel(string fullPath, ICommand openCommand)
    {
        FullPath = fullPath;
        FileName = Path.GetFileName(fullPath);
        Directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
        OpenCommand = openCommand;
    }

    [ObservableProperty]
    private string fullPath;

    [ObservableProperty]
    private string fileName;

    [ObservableProperty]
    private string directory;

    [ObservableProperty]
    private ICommand openCommand;
}
