using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SolRIA.SAFT.Desktop.Models;
using System.IO;
using System.Windows.Input;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class RecentFileItemViewModel : ObservableObject
{
    public RecentFileItemViewModel() { }

    public RecentFileItemViewModel(RecentFileEntry entry, ICommand openCommand, ICommand removeCommand)
    {
        FullPath = entry.FullPath;
        FileName = Path.GetFileName(entry.FullPath);
        Directory = Path.GetDirectoryName(entry.FullPath) ?? string.Empty;
        FileType = entry.FileType;
        OpenCommand = openCommand;
        RemoveCommand = removeCommand;
    }

    public RecentFileType FileType { get; set; }

    public bool IsStocks => FileType == RecentFileType.Stocks;
    public bool IsDocumentsAT => FileType == RecentFileType.DocumentsAT;
    public bool IsTransport => FileType == RecentFileType.Transport;

    public string FileTypeDisplayName => FileType switch
    {
        RecentFileType.Stocks => "Existências (Stocks)",
        RecentFileType.DocumentsAT => "Documentos AT",
        RecentFileType.Transport => "Guias Transporte",
        _ => "SAF-T (PT)"
    };

    public MaterialIconKind Icon => FileType switch
    {
        RecentFileType.Stocks => MaterialIconKind.BarcodeScan,
        RecentFileType.DocumentsAT => MaterialIconKind.CloudDownloadOutline,
        RecentFileType.Transport => MaterialIconKind.TruckFastOutline,
        _ => MaterialIconKind.FileCodeOutline
    };

    public ICommand RemoveCommand { get; set; }

    [ObservableProperty]
    public partial string FullPath { get; set; }

    [ObservableProperty]
    public partial string FileName { get; set; }

    [ObservableProperty]
    public partial string Directory { get; set; }

    [ObservableProperty]
    public partial ICommand OpenCommand { get; set; }
}
