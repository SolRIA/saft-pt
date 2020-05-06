using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class ProductsPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public ProductsPageViewModel(IScreen screen) : base(screen, MenuIds.ERRORS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();

            DoPrintCommand = ReactiveCommand.Create(OnDoPrint);
            GenerateScriptCommand = ReactiveCommand.Create(OnGenerateScript);
            SearchCommand = ReactiveCommand.Create(OnSearch);
        }

        protected override void HandleActivation()
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
                        var prices = invoices_lines.Where(l => l.ProductCode.Equals(p.ProductCode, System.StringComparison.OrdinalIgnoreCase))
                            .Select(l => l.UnitPrice.ToString("N2"))
                            .Distinct()
                            .ToArray();

                        var taxes = invoices_lines.Where(l => l.ProductCode.Equals(p.ProductCode, System.StringComparison.OrdinalIgnoreCase))
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
                        var prices = invoices_lines.Where(l => l.ProductCode.Equals(p.ProductCode, System.StringComparison.OrdinalIgnoreCase))
                            .Select(l => l.UnitPrice.ToString("N2"))
                            .Distinct()
                            .ToArray();

                        var taxes = invoices_lines.Where(l => l.ProductCode.Equals(p.ProductCode, System.StringComparison.OrdinalIgnoreCase))
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

            CollectionView = new DataGridCollectionView(products)
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is Product product)
                    {
                        if (product.ProductCode != null && product.ProductCode.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductDescription != null && product.ProductDescription.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductGroup != null && product.ProductGroup.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductNumberCode != null && product.ProductNumberCode.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (product.ProductType != null && product.ProductType.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };
            CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("ProductType"));
        }

        protected override void HandleDeactivation()
        {

        }

        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateScriptCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        private void OnDoPrint()
        {

        }

        private void OnGenerateScript()
        {

        }

        private void OnSearch()
        {
            CollectionView.Refresh();
        }
    }
}
