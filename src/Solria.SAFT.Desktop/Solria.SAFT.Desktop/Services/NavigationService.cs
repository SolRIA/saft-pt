using Avalonia.Controls;
using SolRIA.SAFT.Desktop.ViewModels;

namespace SolRIA.SAFT.Desktop.Services;

public class NavigationService : INavigationService
{
    Grid _parentNavigationControl;
    public void InitNavigationcontrol(Grid parentNavigationControl)
    {
        _parentNavigationControl ??= parentNavigationControl;
    }
    public void NavigateTo(UserControl control)
    {
        _parentNavigationControl.Children.Clear();
        _parentNavigationControl.Children.Add(control);
    }

    public void NavigateTo<T>(T vm) where T : ViewModelBase
    {
        var vmType = typeof(T);

        if (vmType == typeof(SaftCustomersPageViewModel))
            NavigateTo(new Views.SaftCustomersPageView { DataContext = vm });
        else if (vmType == typeof(SaftErrorPageViewModel))
            NavigateTo(new Views.SaftErrorPageView { DataContext = vm });
        else if (vmType == typeof(SaftHeaderPageViewModel))
            NavigateTo(new Views.SaftHeaderPageView { DataContext = vm });
        else if (vmType == typeof(SaftInvoicesPageViewModel))
            NavigateTo(new Views.SaftInvoicesPageView { DataContext = vm });
        else if (vmType == typeof(SaftMovementOfGoodsPageViewModel))
            NavigateTo(new Views.SaftMovementOfGoodsPageView { DataContext = vm });
        else if (vmType == typeof(SaftPaymentsPageViewModel))
            NavigateTo(new Views.SaftPaymentsPageView { DataContext = vm });
        else if (vmType == typeof(SaftProductsPageViewModel))
            NavigateTo(new Views.SaftProductsPageView { DataContext = vm });
        else if (vmType == typeof(SaftSuppliersPageViewModel))
            NavigateTo(new Views.SaftSuppliersPageView { DataContext = vm });
        else if (vmType == typeof(SaftHeaderPageViewModel))
            NavigateTo(new Views.SaftHeaderPageView { DataContext = vm });
        else if (vmType == typeof(StocksHeaderPageViewModel))
            NavigateTo(new Views.StocksHeaderPageView { DataContext = vm });
        else if (vmType == typeof(StocksProductsPageViewModel))
            NavigateTo(new Views.StocksProductsPageView { DataContext = vm });
        else if (vmType == typeof(SaftTaxesPageViewModel))
            NavigateTo(new Views.SaftTaxesPageView { DataContext = vm });
        else if (vmType == typeof(SaftWorkingDocumentsPageViewModel))
            NavigateTo(new Views.SaftWorkingDocumentsPageView { DataContext = vm });
    }
}
