using DynamicData.Binding;
using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Collections.Generic;
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

            OpenSaftCommand = ReactiveCommand.CreateFromTask(OnOpenSaft, canOpen);
            OpenTransportCommand = ReactiveCommand.Create(OnOpenTransport, canOpen);
            OpenStocksCommand = ReactiveCommand.Create(OnOpenStocks, canOpen);

            OpenMenuCommand = ReactiveCommand.Create<string>(OnOpenMenu);
            OpenRecentFileCommand = ReactiveCommand.CreateFromTask<string>(OnOpenRecentFile);
            ClearRecentFilesCommand = ReactiveCommand.Create(OnClearRecentFiles);

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
            this.WhenValueChanged(x => x.RecentFile)
                .InvokeCommand(OpenRecentFileCommand);

            dialogManager.AddMessage("A iniciar base de dados");
            Task.Run(() =>
            {
                databaseService.InitDatabase();
                dialogManager.AddMessage("");

                RecentFiles = databaseService.GetRecentFiles();

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

        private IEnumerable<string> recentFiles;
        public IEnumerable<string> RecentFiles
        {
            get => recentFiles;
            set => this.RaiseAndSetIfChanged(ref recentFiles, value);
        }

        private string recentFile;
        public string RecentFile
        {
            get => recentFile;
            set => this.RaiseAndSetIfChanged(ref recentFile, value);
        }

        public ReactiveCommand<Unit, Unit> OpenSaftCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenTransportCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenStocksCommand { get; }

        public ReactiveCommand<string, Unit> OpenRecentFileCommand { get; }
        public ReactiveCommand<string, Unit> OpenMenuCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearRecentFilesCommand { get; }

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
                GoTo(new ErrorPageViewModel(this));

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
        private async Task OnOpenRecentFile(string saft_file)
        {
            if (string.IsNullOrWhiteSpace(saft_file) == false)
            {
                await saftValidator.OpenSaftFileV4(saft_file);

                //show error page
                GoTo(new ErrorPageViewModel(this));

                ShowMenu = true;
            }
        }
        private void OnOpenMenu(string menu)
        {
            if (string.IsNullOrWhiteSpace(menu))
                return;

            if (menu.Equals("Erros", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new ErrorPageViewModel(this));
            else if (menu.Equals("Cabeçalho", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new HeaderPageViewModel(this));
            else if(menu.Equals("Clientes", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new CustomersPageViewModel(this));
            else if (menu.Equals("Fornecedores", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new SuppliersPageViewModel(this));
            else if (menu.Equals("Produtos", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new ProductsPageViewModel(this));
            else if (menu.Equals("Impostos", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new TaxesPageViewModel(this));
            else if(menu.Equals("Documentos Faturação", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new InvoicesPageViewModel(this));
        }
        private void GoTo(ViewModelBase vm)
        {
            Router.NavigateAndReset.Execute(vm);
        }

        private void OnClearRecentFiles()
        {
            RecentFiles = null;
            databaseService.ClearRecentFiles();
        }
    }
}
