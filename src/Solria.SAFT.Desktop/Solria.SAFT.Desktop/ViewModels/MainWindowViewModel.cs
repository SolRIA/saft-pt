using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.Views;
using SolRIA.SAFT.Parser.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    readonly IDialogManager dialogManager;
    readonly ISaftValidator saftValidator;
    readonly IDatabaseService databaseService;
    readonly INavigationService navigationService;
    readonly IThemeService themeService;

    private Preferences preferences;

    public MainWindowViewModel()
    {
        DatabaseReady = false;
        ShowMenu = false;

        dialogManager = AppBootstrap.Resolve<IDialogManager>();
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        databaseService = AppBootstrap.Resolve<IDatabaseService>();
        navigationService = AppBootstrap.Resolve<INavigationService>();
        themeService = AppBootstrap.Resolve<IThemeService>() ?? new ThemeService();

        themeService.ThemeChanged += OnThemeChanged;
        UpdateThemeProperties(themeService.CurrentTheme);

        AppVersion = databaseService.GetAppVersion();

        dialogManager.AddMessage("A iniciar base de dados");
        Task.Run(() =>
        {
            databaseService.InitDatabase();
            dialogManager.AddMessage("");

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                RefreshRecentFiles();
                DatabaseReady = true;
                BuildMenu();
            });
        });

        dialogManager.UpdateVersionInfo(AppVersion);
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
    private ObservableCollection<MenuItemViewModel> recentFiles = new();

    [ObservableProperty]
    private ObservableCollection<RecentFileItemViewModel> recentFilesDashboard = new();

    [ObservableProperty]
    private MenuItemViewModel[] menuItems;

    [ObservableProperty]
    private string appVersion;

    [ObservableProperty]
    private string loadedFileName;

    [ObservableProperty]
    private string loadedCompanyName;

    [ObservableProperty]
    private bool hasLoadedFile;

    [ObservableProperty]
    private string currentTheme;

    [ObservableProperty]
    private string currentThemeDisplayName;

    [ObservableProperty]
    private string currentThemeIcon;

    [ObservableProperty]
    private string currentThemeTooltip;

    private void UpdateThemeProperties(string theme)
    {
        CurrentTheme = theme;
        CurrentThemeDisplayName = theme switch
        {
            ThemeService.ThemeLight => "Claro",
            ThemeService.ThemeDark => "Escuro",
            _ => "Sistema"
        };
        CurrentThemeIcon = theme switch
        {
            ThemeService.ThemeLight => "WeatherSunny",
            ThemeService.ThemeDark => "WeatherNight",
            _ => "Laptop"
        };
        CurrentThemeTooltip = $"Tema atual: {CurrentThemeDisplayName} (clique para alternar)";
    }

    private void OnThemeChanged(string newTheme)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            UpdateThemeProperties(newTheme);
            BuildMenu();
        });
    }

    [RelayCommand]
    private void OnSetTheme(string theme)
    {
        themeService.SetTheme(theme);
    }

    [RelayCommand]
    private void OnToggleTheme()
    {
        themeService.ToggleTheme();
    }

    [RelayCommand]
    private void OnGoToHome()
    {
        var view = new MainMenuPageView { DataContext = this };
        navigationService.NavigateTo(view);
    }

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
                    Patterns = ["*.xml"],
                    MimeTypes = ["application/xml"]
                }
            };
            var results = await dialogManager.OpenFileDialog("Ficheiro SAFT-PT", filters: filters);

            if (results == null || results.Length == 0) return;

            var selectedfile = results.First();
            preferences.AddRecentFile(selectedfile);
            Preferences.Save(preferences);
            RefreshRecentFiles();

            await saftValidator.OpenSaftFile(selectedfile);

            dialogManager.SetFileName(selectedfile);
            dialogManager.SetTitle(saftValidator.SaftFile?.Header?.CompanyName);

            LoadedFileName = Path.GetFileName(selectedfile);
            LoadedCompanyName = saftValidator.SaftFile?.Header?.CompanyName ?? "";
            HasLoadedFile = true;

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
        preferences.AddRecentFile(selectedfile);
        Preferences.Save(preferences);
        RefreshRecentFiles();

        await saftValidator.OpenStockFile(selectedfile);

        dialogManager.SetFileName(selectedfile);
        dialogManager.SetTitle(saftValidator.StockFile?.StockHeader?.TaxRegistrationNumber);

        LoadedFileName = Path.GetFileName(selectedfile);
        LoadedCompanyName = saftValidator.StockFile?.StockHeader?.TaxRegistrationNumber ?? "";
        HasLoadedFile = true;

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
        else if (menu.Equals("Movimentos contabilísticos", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new SaftGeneralLedgerEntriesPageViewModel());
    }

    [RelayCommand]
    private void OnOpenMenuStocks(string menu)
    {
        if (string.IsNullOrWhiteSpace(menu))
            return;

        if (menu.Equals("Erros", StringComparison.OrdinalIgnoreCase))
            navigationService.NavigateTo(new StocksHeaderPageViewModel());
        else if (menu.Equals("Cabeçalho", StringComparison.OrdinalIgnoreCase))
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
        if (string.IsNullOrWhiteSpace(saft_file) || File.Exists(saft_file) == false)
        {
            await dialogManager.ShowMessageDialogAsync("Aviso", $"O ficheiro não foi encontrado:\n{saft_file}", MessageDialogType.Warning);
            return;
        }

        try
        {
            preferences.AddRecentFile(saft_file);
            Preferences.Save(preferences);
            RefreshRecentFiles();

            await saftValidator.OpenSaftFile(saft_file);

            dialogManager.SetFileName(saft_file);
            dialogManager.SetTitle(saftValidator.SaftFile?.Header?.CompanyName);

            LoadedFileName = Path.GetFileName(saft_file);
            LoadedCompanyName = saftValidator.SaftFile?.Header?.CompanyName ?? "";
            HasLoadedFile = true;

            //show resume
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
    private void OnClearRecentFiles()
    {
        RecentFiles.Clear();
        RecentFilesDashboard.Clear();
        preferences.RecentFiles.Clear();
        Preferences.Save(preferences);
        BuildMenu();
    }

    private void RefreshRecentFiles()
    {
        preferences = Preferences.Load();

        var recentItems = preferences.RecentFiles.Select(r => new MenuItemViewModel
        {
            Header = Path.GetFileName(r),
            Command = OpenRecentSaftFileCommand,
            CommandParameter = r
        });

        RecentFiles = new ObservableCollection<MenuItemViewModel>(recentItems);
        if (RecentFiles.Count > 0)
        {
            RecentFiles.Add(new() { Header = "Limpar histórico", Command = ClearRecentFilesCommand });
        }

        var dashboardItems = preferences.RecentFiles.Select(r => new RecentFileItemViewModel(r, OpenRecentSaftFileCommand));
        RecentFilesDashboard = new ObservableCollection<RecentFileItemViewModel>(dashboardItems);
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

    [RelayCommand]
    private void OnOpenAtPortal()
    {
        OpenUrl("https://www.portaldasfinancas.gov.pt");
    }

    [RelayCommand]
    private void OnOpenAtUploadSaft()
    {
        OpenUrl("https://faturas.portaldasfinancas.gov.pt/");
    }

    private static void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            if (OperatingSystem.IsWindows())
            {
                var safeUrl = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {safeUrl}") { CreateNoWindow = true });
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", url);
            }
        }
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
                    new() { Header = "Início", Command = GoToHomeCommand },
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
                    new() { Header = "Documentos Movimentação", Command = OpenMenuSaftCommand, CommandParameter = "Documentos Movimentação" },
                    new() { Header = "Movimentos contabilísticos", Command = OpenMenuSaftCommand, CommandParameter = "Movimentos contabilísticos" }
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
            },
            new MenuItemViewModel
            {
                Header = "_Tema",
                Items =
                [
                    new() { Header = (CurrentTheme == "System" ? "✓ Seguir Sistema" : "   Seguir Sistema"), Command = SetThemeCommand, CommandParameter = "System" },
                    new() { Header = (CurrentTheme == "Light" ? "✓ Claro" : "   Claro"), Command = SetThemeCommand, CommandParameter = "Light" },
                    new() { Header = (CurrentTheme == "Dark" ? "✓ Escuro" : "   Escuro"), Command = SetThemeCommand, CommandParameter = "Dark" }
                ]
            },
            new MenuItemViewModel
            {
                Header = "_Ajuda",
                Items =
                [
                    new() { Header = "Portal das Finanças (Oficial)", Command = OpenAtPortalCommand },
                    new() { Header = "Upload SAF-T (e-Fatura)", Command = OpenAtUploadSaftCommand }
                ]
            }
        ];
    }
}
