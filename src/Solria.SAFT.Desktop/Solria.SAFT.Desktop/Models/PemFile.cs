using CommunityToolkit.Mvvm.ComponentModel;

namespace SolRIA.SAFT.Desktop.Models;

public partial class PemFile : ObservableObject
{
    [ObservableProperty]
    private bool privateKey;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string pemText;
    [ObservableProperty]
    private string rsaSettings;

    public int Id { get; set; }
}
