using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class ProductsPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;
        readonly IDialogManager dialogManager;

        public ProductsPageViewModel(IScreen screen) : base(screen, MenuIds.PRODUCTS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
        }

        protected override void HandleActivation()
        {
            IsLoading = true;

            Task.Run(() =>
            {
                var products = new List<Product>();
                if (saftValidator?.SaftFileV4?.MasterFiles?.Product != null)
                {
                    var saft_products = saftValidator.SaftFileV4.MasterFiles.Product;

                    //calculated fields
                    var invoices_lines = saftValidator.SaftFileV4?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line);
                    if (invoices_lines != null)
                    {
                        foreach (var p in saft_products)
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

                    //create binding products
                    foreach (var c in saft_products)
                    {
                        products.Add(new Product
                        {
                            ProductCode = c.ProductCode,
                            ProductDescription = c.ProductDescription,
                            ProductGroup = c.ProductGroup,
                            ProductNumberCode = c.ProductNumberCode,
                            ProductType = c.ProductType.ToString(),
                            CustomsDetails = new CustomsDetails
                            {
                                CNCode = c.CustomsDetails?.CNCode,
                                UNNumber = c.CustomsDetails?.UNNumber
                            },
                            Prices = c.Prices,
                            Taxes = c.Taxes
                        });
                    }
                }
                else if (saftValidator?.SaftFileV3?.MasterFiles?.Customer != null)
                {
                    var saft_products = saftValidator.SaftFileV3.MasterFiles.Product;

                    //calculated fields
                    var invoices_lines = saftValidator.SaftFileV4?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line);
                    if (invoices_lines != null)
                    {
                        foreach (var p in saft_products)
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

                    //create binding products
                    foreach (var c in saft_products)
                    {
                        products.Add(new Product
                        {
                            ProductCode = c.ProductCode,
                            ProductDescription = c.ProductDescription,
                            ProductGroup = c.ProductGroup,
                            ProductNumberCode = c.ProductNumberCode,
                            ProductType = c.ProductType.ToString(),
                            Prices = c.Prices,
                            Taxes = c.Taxes
                        });
                    }
                }

                return products;
            }).ContinueWith(async p =>
            {
                var products = await p;

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
                            if (product.ProductType != null && product.ProductType.Contains(Filter, StringComparison.OrdinalIgnoreCase))
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
                .InvokeCommand(SearchCommand);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void HandleDeactivation()
        {

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
