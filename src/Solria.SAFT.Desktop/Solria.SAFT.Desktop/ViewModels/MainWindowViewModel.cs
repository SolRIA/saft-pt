using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.Views;
using Splat;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        readonly IDialogManager dialogManager;
        readonly ISaftValidator saftValidator;
        readonly IDatabaseService databaseService;

        public RoutingState Router { get; }

        public MainWindowViewModel()
        {
            DatabaseReady = false;

            Router = new RoutingState();

            ShowMenu = false;

            dialogManager = Locator.Current.GetService<IDialogManager>();
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            databaseService = Locator.Current.GetService<IDatabaseService>();

            var canOpen = this.WhenValueChanged(x => x.DatabaseReady)
                .ObserveOn(RxApp.MainThreadScheduler);

            var canClearFiles = this.WhenAnyValue(x => x.RecentFiles, recentFiles => recentFiles != null && recentFiles.Count() > 0);

            ExitCommand = ReactiveCommand.Create(OnExit);
            OpenSaftCommand = ReactiveCommand.CreateFromTask(OnOpenSaft, canOpen);
            OpenTransportCommand = ReactiveCommand.Create(OnOpenTransport, canOpen);
            OpenStocksCommand = ReactiveCommand.Create(OnOpenStocks, canOpen);

            OpenRecentFileCommand = ReactiveCommand.CreateFromTask<string>(OnOpenRecentSaftFile);
            OpenMenuCommand = ReactiveCommand.Create<string>(OnOpenMenu);
            ClearRecentFilesCommand = ReactiveCommand.Create(OnClearRecentFiles);
            OpenPemDialogCommand = ReactiveCommand.CreateFromTask(OnOpenPemDialog);
            OpenHashDialogCommand = ReactiveCommand.CreateFromTask(OnOpenHashDialog);

            MenuHeader = new string[]
            {
                "Erros",
                "Cabeçalho"
            };
            MenuTables = new string[]
            {
                "Clientes",
                "Fornecedores",
                "Produtos",
                "Impostos"
            };
            MenuInvoices = new string[]
            {
                "Documentos Faturação"
            };

            this.WhenValueChanged(x => x.SelectedMenu)
                .InvokeCommand(OpenMenuCommand);

            dialogManager.AddMessage("A iniciar base de dados");
            Task.Run(() =>
            {
                databaseService.InitDatabase();
                dialogManager.AddMessage("");

                var recentFiles = databaseService.GetRecentFiles();

                var recentMenu = new List<MenuItemViewModel>(
                    recentFiles.Select(r => new MenuItemViewModel { Header = r, Command = OpenRecentFileCommand, CommandParameter = r }))
                {
                    new MenuItemViewModel { Header = "Limpar", Command = ClearRecentFilesCommand }
                };

                RecentFiles = new ObservableCollection<MenuItemViewModel>();
                var recentFilesMenu = new MenuItemViewModel[]
                {
                    new MenuItemViewModel
                    {
                        Header = "_Recentes",
                        Items = recentMenu
                    },
                };
                RecentFiles.AddRange(recentFilesMenu);

                DatabaseReady = true;
            });

            dialogManager.UpdateVersionInfo(databaseService.GetAppVersion());
        }

        private bool databaseReady;
        public bool DatabaseReady
        {
            get => databaseReady;
            set => this.RaiseAndSetIfChanged(ref databaseReady, value);
        }

        private bool showMenu;
        public bool ShowMenu
        {
            get => showMenu;
            set => this.RaiseAndSetIfChanged(ref showMenu, value);
        }

        private string[] menuHeader;
        public string[] MenuHeader
        {
            get => menuHeader;
            set => this.RaiseAndSetIfChanged(ref menuHeader, value);
        }

        private string[] menuTables;
        public string[] MenuTables
        {
            get => menuTables;
            set => this.RaiseAndSetIfChanged(ref menuTables, value);
        }

        private string[] menuInvoices;
        public string[] MenuInvoices
        {
            get => menuInvoices;
            set => this.RaiseAndSetIfChanged(ref menuInvoices, value);
        }

        private string selectedMenu;
        public string SelectedMenu
        {
            get => selectedMenu;
            set => this.RaiseAndSetIfChanged(ref selectedMenu, value);
        }

        private ObservableCollection<MenuItemViewModel> recentFiles;
        public ObservableCollection<MenuItemViewModel> RecentFiles
        {
            get => recentFiles;
            set => this.RaiseAndSetIfChanged(ref recentFiles, value);
        }

        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenSaftCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenTransportCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenStocksCommand { get; }

        public ReactiveCommand<string, Unit> OpenMenuCommand { get; }
        public ReactiveCommand<string, Unit> OpenRecentFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearRecentFilesCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenPemDialogCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenHashDialogCommand { get; }

        private void OnExit()
        {
            dialogManager.CloseApp();
        }
        private async Task OnOpenSaft()
        {
            var filters = new List<Avalonia.Controls.FileDialogFilter>
            {
                new Avalonia.Controls.FileDialogFilter
                {
                    Extensions = new List<string> { "xml" },
                    Name = "SAFT-PT"
                }
            };
            var results = await dialogManager.OpenFileDialog("Ficheiro SAFT-PT", filters: filters);

            if (results != null && results.Length > 0)
            {
                var saft_file = results.First();
                databaseService.AddRecentFile(saft_file);
                await saftValidator.OpenSaftFileV4(saft_file);

                //show error page
                //GoTo(new ErrorPageViewModel(this));

                var view = new DialogResume();
                var vm = new DialogResumeViewModel(this);
                vm.Init();
                view.DataContext = vm;

                await dialogManager.ShowChildDialogAsync(view);

                ShowMenu = true;
            }
        }
        private void OnOpenTransport()
        {
            //GoTo(new BillingDocumentsPageViewModel(HostScreen));
        }
        private void OnOpenStocks()
        {
            //GoTo(new BillingDocumentsPageViewModel(HostScreen));
        }
        private void OnOpenMenu(string menu)
        {
            if (string.IsNullOrWhiteSpace(menu))
                return;

            if (menu.Equals("Erros", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new ErrorPageViewModel(this));
            else if (menu.Equals("Cabeçalho", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new HeaderPageViewModel(this));
            else if (menu.Equals("Clientes", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new CustomersPageViewModel(this));
            else if (menu.Equals("Fornecedores", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new SuppliersPageViewModel(this));
            else if (menu.Equals("Produtos", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new ProductsPageViewModel(this));
            else if (menu.Equals("Impostos", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new TaxesPageViewModel(this));
            else if (menu.Equals("Documentos Faturação", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new InvoicesPageViewModel(this));
        }
        private void GoTo(ViewModelBase vm)
        {
            Router.NavigateAndReset.Execute(vm);
        }

        private async Task OnOpenRecentSaftFile(string saft_file)
        {
            if (File.Exists(saft_file) == true)
            {
                await saftValidator.OpenSaftFileV4(saft_file);

                //show resume
                var view = new DialogResume();
                var vm = new DialogResumeViewModel(this);
                vm.Init();
                view.DataContext = vm;

                await dialogManager.ShowChildDialogAsync(view);

                ShowMenu = true;
            }
        }
        private void OnClearRecentFiles()
        {
            RecentFiles = null;
            databaseService.ClearRecentFiles();
        }
        private async Task OnOpenPemDialog()
        {
            var view = new DialogConvertPemKey();
            var vm = new DialogConvertPemKeyViewModel();
            vm.Init();
            view.DataContext = vm;

            await dialogManager.ShowChildDialogAsync(view);
        }
        private async Task OnOpenHashDialog()
        {
            var view = new DialogHashTest();
            var vm = new DialogHashTestViewModel();
            vm.Init();
            view.DataContext = vm;

            await dialogManager.ShowChildDialogAsync(view);
        }
    }


    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public System.Windows.Input.ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public IList<MenuItemViewModel> Items { get; set; }
    }
}
