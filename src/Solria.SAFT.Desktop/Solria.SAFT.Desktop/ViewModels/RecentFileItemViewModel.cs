using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class RecentFileItemViewModel : ObservableObject
{
    public RecentFileItemViewModel() { }

    public RecentFileItemViewModel(string fullPath, ICommand openCommand)
    {
        FullPath = fullPath;
        FileName = Path.GetFileName(fullPath);
        Directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
        OpenCommand = openCommand;
    }

    [ObservableProperty]
    public partial string FullPath { get; set; }

    [ObservableProperty]
    public partial string FileName { get; set; }

    [ObservableProperty]
    public partial string Directory { get; set; }

    [ObservableProperty]
    public partial ICommand OpenCommand { get; set; }
}
