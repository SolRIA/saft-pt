using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.Views;
using SolRIA.SAFT.Parser.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    readonly IDialogManager dialogManager;
    readonly ISaftValidator saftValidator;
    readonly IDatabaseService databaseService;
    readonly INavigationService navigationService;

    private Preferences preferences;

    public MainWindowViewModel()
    {
        DatabaseReady = false;

        ShowMenu = false;

        dialogManager = AppBootstrap.Resolve<IDialogManager>();
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        databaseService = AppBootstrap.Resolve<IDatabaseService>();
        navigationService = AppBootstrap.Resolve<INavigationService>();

        dialogManager.AddMessage("A iniciar base de dados");
        Task.Run(() =>
        {
            databaseService.InitDatabase();
            dialogManager.AddMessage("");

            preferences = Preferences.Load();

            var recentFilesNames = preferences.RecentFiles.Select(r => new MenuItemViewModel { Header = r, Command = OpenRecentSaftFileCommand, CommandParameter = r });

            RecentFiles = new ObservableCollection<MenuItemViewModel>(recentFilesNames)
            {
                new() { Header = "Limpar", Command = ClearRecentFilesCommand }
            };

            DatabaseReady = true;
        }).ContinueWith(t =>
        {
            BuildMenu();
        });

        dialogManager.UpdateVersionInfo(databaseService.GetAppVersion());
    }

    [ObservableProperty]
    private bool databaseReady;

    [ObservableProperty]
    private bool showMenu;

    [ObservableProperty]
    private bool isSaft;

    [ObservableProperty]
    private bool isStock;

    [ObservableProperty]
    private bool isTransport;

    [ObservableProperty]
    private string selectedSaftMenu;

    [ObservableProperty]
    private string selectedStocksMenu;

    [ObservableProperty]
    private string selectedTransportMenu;

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> recentFiles;

    [ObservableProperty]
    private MenuItemViewModel[] menuItems;


    [RelayCommand]
    private void OnExit()
    {
        dialogManager.CloseApp();
    }
    [RelayCommand]
    private async Task OnOpenSaft()
    {
        try
        {
            var filters = new FilePickerFileType[]
            {
                new("Ficheiro SAFT-PT")
                {
                    Patterns = new[] { "*.xml" },
                    MimeTypes = new[] { "application/xml" }
                }
            };
            var results = await dialogManager.OpenFileDialog("Ficheiro SAFT-PT", filters: filters);

            if (results == null || results.Length == 0) return;

            var selectedfile = results.First();
            preferences.RecentFiles.Add(selectedfile);
            Preferences.Save(preferences);
            AddRecentFile(selectedfile);

            await saftValidator.OpenSaftFile(selectedfile);

            dialogManager.SetFileName(selectedfile);
            dialogManager.SetTitle(saftValidator.SaftFile?.Header?.CompanyName);

            var vm = new DialogSaftResumeViewModel();
            vm.Init();

            await dialogManager.ShowChildDialogAsync(vm);

            ShowMenu = true;
            IsSaft = true;
        }
        catch (Exception ex)
        {
            await dialogManager.ShowMessageDialogAsync("Erro", ex.Message, MessageDialogType.Error);
        }
    }

    [RelayCommand]
    private void OnOpenTransport()
    {

    }

    [RelayCommand]
    private async Task OnOpenStocks()
    {
        var filters = new FilePickerFileType[]
        {
            new("Ficheiro SAFT-PT")
            {
                Patterns = ["*.xml", "*.csv"],
                MimeTypes = ["application/xml", "application/csv"]
            }
        };
        var results = await dialogManager.OpenFileDialog("Ficheiro Existências", filters: filters);

        if (results == null || results.Length == 0) return;

        var selectedfile = results.First();

        await saftValidator.OpenStockFile(selectedfile);

        dialogManager.SetFileName(selectedfile);
        dialogManager.SetTitle(saftValidator.StockFile?.StockHeader?.TaxRegistrationNumber);

        var vm = new StocksProductsPageViewModel();
        navigationService.NavigateTo(new StocksProductsPageView { DataContext = vm });

        ShowMenu = true;
        IsStock = true;
    }

    [RelayCommand]
    private void OnOpenMenuSaft(string menu)
    {
        if (string.IsNullOrWhiteSpace(menu))
            return;

        if (menu.Equals("Erros", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftErrorPageViewModel());
        else if (menu.Equals("Cabeçalho", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftHeaderPageViewModel());
        else if (menu.Equals("Clientes", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftCustomersPageViewModel());
        else if (menu.Equals("Fornecedores", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftSuppliersPageViewModel());
        else if (menu.Equals("Produtos", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftProductsPageViewModel());
        else if (menu.Equals("Impostos", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftTaxesPageViewModel());
        else if (menu.Equals("Documentos Faturação", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftInvoicesPageViewModel());
        else if (menu.Equals("Pagamentos", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftPaymentsPageViewModel());
        else if (menu.Equals("Documentos Conferência", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftWorkingDocumentsPageViewModel());
        else if (menu.Equals("Documentos Movimentação", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftMovementOfGoodsPageViewModel());
    }

    [RelayCommand]
    private void OnOpenMenuStocks(string menu)
    {
        if (string.IsNullOrWhiteSpace(menu))
            return;

        if (menu.Equals("Cabeçalho", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new StocksHeaderPageViewModel());
        else if (menu.Equals("Produtos", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new StocksProductsPageViewModel());
    }

    [RelayCommand]
    private void OnOpenMenuTransport(string menu)
    {
        if (string.IsNullOrWhiteSpace(menu))
            return;
    }

    [RelayCommand]
    private async Task OnOpenRecentSaftFile(string saft_file)
    {
        if (File.Exists(saft_file) == false) return;

        await saftValidator.OpenSaftFile(saft_file);

        dialogManager.SetFileName(saft_file);
        dialogManager.SetTitle(saftValidator.SaftFile?.Header?.CompanyName);

        //show resume
        var vm = new DialogSaftResumeViewModel();
        vm.Init();

        await dialogManager.ShowChildDialogAsync(vm);

        ShowMenu = true;
        IsSaft = true;
    }

    [RelayCommand]
    private void OnClearRecentFiles()
    {
        RecentFiles.Clear();
        preferences.RecentFiles.Clear();
        Preferences.Save(preferences);
    }

    [RelayCommand]
    private void AddRecentFile(string filename)
    {
        RecentFiles.Add(new() { Header = filename, Command = OpenSaftCommand });

        if (RecentFiles.Count == 1)
            RecentFiles.Add(new() { Header = "Limpar", Command = ClearRecentFilesCommand });
    }

    [RelayCommand]
    private async Task OnOpenPemDialog()
    {
        var vm = new DialogConvertPemKeyViewModel();
        vm.Init();

        await dialogManager.ShowChildDialogAsync(vm);
    }

    [RelayCommand]
    private async Task OnOpenHashDialog()
    {
        var vm = new DialogHashTestViewModel();
        vm.Init();

        await dialogManager.ShowChildDialogAsync(vm);
    }

    [RelayCommand]
    private async Task OnOpenDocumentsAT()
    {
        var filters = new FilePickerFileType[]
        {
            new("Documentos AT")
            {
                Patterns = ["*.json"],
                MimeTypes = ["application/json"]
            }
        };

        var results = await dialogManager.OpenFileDialog("Documents AT", filters: filters);

        if (results == null || results.Length == 0) return;

        var selectedfile = results.First();

        var vm = new DialogReadInvoicesATViewModel();
        vm.Init(selectedfile);

        await dialogManager.ShowChildDialogAsync(vm);
    }

    private void BuildMenu()
    {
        MenuItems =
        [
            new MenuItemViewModel
            {
                Header = "_Ficheiro",
                Items =
                [
                    new() { Header = "Abrir _SAFT", Command = OpenSaftCommand },
                    new() { Header = "Abrir _Transporte", Command = OpenTransportCommand },
                    new() { Header = "Abrir _Stocks", Command = OpenStocksCommand },
                    new() { Header = "Abrir documentos AT", Command = OpenDocumentsATCommand },
                    new() { Header = "Recentes", Items = RecentFiles },
                    new() { Header = "_Sair", Command = ExitCommand },
                ]
            },
            new MenuItemViewModel
            {
                Header = "_SAFT",
                Items =
                [
                    new() { Header = "Erros", Command = OpenMenuSaftCommand, CommandParameter = "Erros" },
                    new() { Header = "Cabeçalho", Command = OpenMenuSaftCommand, CommandParameter = "Cabeçalho" },
                    new() { Header = "Clientes", Command = OpenMenuSaftCommand, CommandParameter = "Clientes" },
                    new() { Header = "Fornecedores", Command = OpenMenuSaftCommand, CommandParameter = "Fornecedores" },
                    new() { Header = "Produtos", Command = OpenMenuSaftCommand, CommandParameter = "Produtos" },
                    new() { Header = "Impostos", Command = OpenMenuSaftCommand, CommandParameter = "Impostos" },
                    new() { Header = "Documentos Faturação", Command = OpenMenuSaftCommand, CommandParameter = "Documentos Faturação" },
                    new() { Header = "Pagamentos", Command = OpenMenuSaftCommand, CommandParameter = "Pagamentos" },
                    new() { Header = "Documentos Conferência", Command = OpenMenuSaftCommand, CommandParameter = "Documentos Conferência" },
                    new() { Header = "Documentos Movimentação", Command = OpenMenuSaftCommand, CommandParameter = "Documentos Movimentação" }
                ]
            },
            new MenuItemViewModel
            {
                Header = "_Stocks",
                Items =
                [
                    new() { Header = "Erros", Command = OpenMenuStocksCommand, CommandParameter = "Erros" },
                    new() { Header = "Cabeçalho", Command = OpenMenuStocksCommand, CommandParameter = "Cabeçalho" },
                    new() { Header = "Produtos", Command = OpenMenuStocksCommand, CommandParameter = "Produtos" }
                ]
            },
            new MenuItemViewModel
            {
                Header = "_Transporte",
                Items = []
            },
            new MenuItemViewModel
            {
                Header = "_Ferramentas",
                Items =
                [
                    new() { Header = "Ler .pem", Command = OpenPemDialogCommand },
                    new() { Header = "Testar Hash", Command = OpenHashDialogCommand }
                ]
            }
        ];
    }
}
