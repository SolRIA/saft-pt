using Avalonia.Controls;
using SolRIA.SAFT.Desktop.ViewModels;

namespace SolRIA.SAFT.Desktop.Services;

public interface INavigationService
{
    void InitNavigationcontrol(Grid parentNavigationControl);
    void NavigateTo(UserControl control);
    void NavigateTo<T>(T vm) where T : ViewModelBase;
}
