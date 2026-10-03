using Avalonia.Controls;
using SolRIA.SAFT.Desktop.ViewModels;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using SolRIA.SAFT.Desktop.Controls;
using SolRIA.SAFT.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolRIA.SAFT.Desktop.Services;

public class NavigationService : INavigationService
{
    Grid _parentNavigationControl;
    private readonly Dictionary<Type, Window> detachedWindows = [];
    public void InitNavigationcontrol(Grid parentNavigationControl)
    {
        _parentNavigationControl ??= parentNavigationControl;
    }
    public void NavigateTo(UserControl control)
    {
        if (ActivateDetachedWindow(control.GetType())) return;
        _parentNavigationControl.Children.Clear();
        _parentNavigationControl.Children.Add(control);
    }

    private bool ActivateDetachedWindow(Type pageType)
    {
        if (!detachedWindows.TryGetValue(pageType, out var window)) return false;
        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
        window.Activate();
        return true;
    }

    public void DetachCurrentPage()
    {
        if (_parentNavigationControl?.Children.SingleOrDefault() is not UserControl page
            || page is MainMenuPageView
            || TopLevel.GetTopLevel(_parentNavigationControl) is not MainWindow owner
            || owner.DataContext is not MainWindowViewModel mainViewModel) return;

        var header = page.GetLogicalDescendants().OfType<PageHeader>().FirstOrDefault();
        if (header == null || !header.CanDetach || ActivateDetachedWindow(page.GetType())) return;

        var window = new Window
        {
            Title = "SolRIA SAF-T | " + header.Title,
            Icon = owner.Icon,
            Width = 1100,
            Height = 700,
            CanResize = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var screen = owner.Screens.ScreenFromWindow(owner) ?? owner.Screens.Primary;
        if (screen != null)
        {
            window.Width = Math.Min(window.Width, screen.WorkingArea.Width / screen.Scaling);
            window.Height = Math.Min(window.Height, screen.WorkingArea.Height / screen.Scaling);
        }

        var pageType = page.GetType();
        window.Closed += (_, _) =>
        {
            detachedWindows.Remove(pageType);
            window.Content = null;
            header.CanDetach = true;
        };

        try
        {
            _parentNavigationControl.Children.Remove(page);
            header.CanDetach = false;
            window.Content = page;
            detachedWindows.Add(pageType, window);
            window.Show(owner);
            NavigateTo(new MainMenuPageView { DataContext = mainViewModel });
        }
        catch (Exception ex)
        {
            detachedWindows.Remove(pageType);
            window.Content = null;
            window.Close();
            header.CanDetach = true;
            _parentNavigationControl.Children.Clear();
            _parentNavigationControl.Children.Add(page);
            owner.ShowNotification("Não foi possível abrir a janela", ex.Message, NotificationType.Error);
        }
    }

    public void CloseDetachedWindows()
    {
        foreach (var window in detachedWindows.Values.ToArray()) window.Close();
    }

    public void NavigateTo<T>(T vm) where T : ViewModelBase
    {
        var vmType = typeof(T);
        var viewType = typeof(MainWindow).Assembly.GetType(
            typeof(MainWindow).Namespace + "." + vmType.Name.Replace("ViewModel", "View"));
        if (viewType != null && ActivateDetachedWindow(viewType)) return;

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
        else if (vmType == typeof(StocksHeaderPageViewModel))
            NavigateTo(new Views.StocksHeaderPageView { DataContext = vm });
        else if (vmType == typeof(StocksProductsPageViewModel))
            NavigateTo(new Views.StocksProductsPageView { DataContext = vm });
        else if (vmType == typeof(SaftTaxesPageViewModel))
            NavigateTo(new Views.SaftTaxesPageView { DataContext = vm });
        else if (vmType == typeof(SaftWorkingDocumentsPageViewModel))
            NavigateTo(new Views.SaftWorkingDocumentsPageView { DataContext = vm });
        else if (vmType == typeof(SaftGeneralLedgerEntriesPageViewModel))
            NavigateTo(new Views.SaftGeneralLedgerEntriesPageView { DataContext = vm });
    }
}
