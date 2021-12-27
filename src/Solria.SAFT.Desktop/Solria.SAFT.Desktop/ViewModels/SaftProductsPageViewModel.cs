using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Parser.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftProductsPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;
        readonly IDialogManager dialogManager;

        public SaftProductsPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_PRODUCTS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            IsLoading = true;

            ToolTip = new ProductToolTipService();

            var products = saftValidator.SaftFile.MasterFiles.Product ?? Array.Empty<Product>();

            //calculated fields
            var invoices_lines = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line);
            if (invoices_lines != null)
            {
                foreach (var p in products)
                {
                    var prices = invoices_lines.Where(l => l.ProductCode.Equals(p.ProductCode, StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.UnitPrice.ToString("N3"))
                        .Distinct()
                        .ToArray();

                    var taxes = invoices_lines.Where(l => l.ProductCode.Equals(p.ProductCode, StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.Tax.TaxCode)
                        .Distinct()
                        .ToArray();

                    if (prices != null && prices.Length > 0)
                        p.Prices = prices.Aggregate((i, j) => i + " | " + j);

                    if (taxes != null && taxes.Length > 0)
                        p.Taxes = taxes.Aggregate((i, j) => i + " | " + j);
                }
            }

            CollectionView = new DataGridCollectionView(products)
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is Product product)
                    {
                        if (product.ProductCode != null && product.ProductCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductDescription != null && product.ProductDescription.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductGroup != null && product.ProductGroup.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductNumberCode != null && product.ProductNumberCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductType.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };

            CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("ProductType"));

            IsLoading = false;

            this.WhenAnyValue(x => x.Filter)
            .Throttle(TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler)
            .InvokeCommand(SearchCommand)
            .DisposeWith(disposables);
        }

        protected override void HandleDeactivation()
        {

        }

        private ProductToolTipService toolTip;
        public ProductToolTipService ToolTip
        {
            get => toolTip;
            set => this.RaiseAndSetIfChanged(ref toolTip, value);
        }

        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchClearCommand { get; }

        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<Product> products)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar produtos",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Produtos.csv",
                    ".csv");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var c in products)
                    {
                        stringBuilder.AppendLine($"{c.ProductCode};{c.ProductDescription};;{c.Prices};{c.ProductNumberCode};{c.ProductGroup};{c.Taxes}");
                    }

                    File.WriteAllText(file, stringBuilder.ToString());
                }
            }
        }

        private void OnSearch(string _)
        {
            CollectionView?.Refresh();
        }
        private void OnSearchClear()
        {
            Filter = null;
        }
    }
}
