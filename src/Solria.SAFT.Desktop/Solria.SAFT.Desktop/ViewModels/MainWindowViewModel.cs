using DynamicData.Binding;
using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        readonly IDialogManager dialogManager;
        readonly ISaftValidator saftValidator;

        public RoutingState Router { get; }

        public MainWindowViewModel()
        {
            Router = new RoutingState();

            ShowMenu = false;

            dialogManager = Locator.Current.GetService<IDialogManager>();
            saftValidator = Locator.Current.GetService<ISaftValidator>();

            OpenSaftCommand = ReactiveCommand.CreateFromTask(OnOpenSaft);
            OpenTransportCommand = ReactiveCommand.Create(OnOpenTransport);
            OpenStocksCommand = ReactiveCommand.Create(OnOpenStocks);

            OpenMenuCommand = ReactiveCommand.Create<string>(OnOpenMenu);

            MenuHeader = new string[]
            {
                "Erros",
                "Cabeçalho"
            };
            MenuTables = new string[]
            {
                "Clientes",
                "Produtos",
                "Impostos"
            };
            this.WhenValueChanged(x => x.SelectedMenu)
                .InvokeCommand(OpenMenuCommand);
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

        private string selectedMenu;
        public string SelectedMenu
        {
            get => selectedMenu;
            set => this.RaiseAndSetIfChanged(ref selectedMenu, value);
        }

        public ReactiveCommand<Unit, Unit> OpenSaftCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenTransportCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenStocksCommand { get; }

        public ReactiveCommand<string, Unit> OpenMenuCommand { get; }

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
                await saftValidator.OpenSaftFileV4(results.First());

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
            else if (menu.Equals("Produtos", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new ProductsPageViewModel(this));
            else if (menu.Equals("Impostos", System.StringComparison.OrdinalIgnoreCase))
                GoTo(new TaxesPageViewModel(this));
        }
        private void GoTo(ViewModelBase vm)
        {
            Router.NavigateAndReset.Execute(vm);
        }
    }
}
