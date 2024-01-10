using CommunityToolkit.Mvvm.ComponentModel;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string filter;
}
