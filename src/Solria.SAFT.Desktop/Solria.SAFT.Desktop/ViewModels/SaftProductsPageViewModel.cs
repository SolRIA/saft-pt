using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Infrastructure;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftProductsPageViewModel : ViewModelBase
{
    readonly ISaftValidator saftValidator;
    readonly IDialogManager dialogManager;

    private Product[] allProducts;
    public SaftProductsPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        ToolTip = new ProductToolTipService();

        allProducts = saftValidator.SaftFile.MasterFiles.Product ?? [];

        if (allProducts.Length == 0)
        {
            dialogManager.ShowNotification("Aviso", "Não existem produtos definidos no ficheiro SAFT.", Avalonia.Controls.Notifications.NotificationType.Warning);
            return;
        }

        IsLoading = true;

        //calculated fields
        var documentLines = GetDocumentLines();
        if (documentLines.Length > 0)
        {
            ProcessProducts(documentLines);
        }

        Products = [.. allProducts];

        IsLoading = false;
    }

    [ObservableProperty]
    public partial ProductToolTipService ToolTip { get; set; }

    [ObservableProperty]
    public partial IList<Product> Products { get; set; }

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Products == null || Products.Count == 0) return;
        var request = ReportRequest.FromRows(ReportTemplate.Produtos, "Produtos",
            ["Código", "Descrição", "Preço", "Código de Barras", "Grupo", "IVA", "Preço com IVA"],
            Products.Select(c => new object[] { c.ProductCode, c.ProductDescription, c.Prices, c.ProductNumberCode, c.ProductGroup, c.Taxes, c.PricesWithVat }));
        await AppBootstrap.Resolve<IReportService>().ShowAsync(request);
    }

    [RelayCommand]
    private void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Filter))
        {
            Products = [.. allProducts];
            return;
        }

        Products = [.. allProducts.Where(d => FilterEntries(d, Filter))];
    }
    private static bool FilterEntries(Product entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.ProductCode) == false && entry.ProductCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(entry.ProductDescription) == false && entry.ProductDescription.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(entry.ProductGroup) == false && entry.ProductGroup.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(entry.ProductNumberCode) == false && entry.ProductNumberCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || entry.ProductType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    [RelayCommand]
    private void OnSearchClear()
    {
        Filter = null;
        OnSearch();
    }

    [RelayCommand]
    private void OnGenerateProductsFromDocumentLines()
    {
        var documentLines = GetDocumentLines();
        if (documentLines.Length == 0)
        {
            dialogManager.ShowNotification("Aviso", "Não existem linhas de documentos no ficheiro SAFT.", Avalonia.Controls.Notifications.NotificationType.Warning);
            return;
        }

        IsLoading = true;

        allProducts = [.. documentLines
            .Where(l => !string.IsNullOrWhiteSpace(l.ProductCode))
            .GroupBy(l => l.ProductCode, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var first = g.First();
                var description = g.Select(x => x.ProductDescription).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d))
                    ?? g.Select(x => x.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d));
                return new Product
                {
                    ProductCode = first.ProductCode,
                    ProductDescription = description,
                    Prices = first.UnitPrice.ToString("N3"),
                    ProductType = ProductType.P
                };
            })];

        ProcessProducts(documentLines);
        OnSearch();

        IsLoading = false;
    }

    private readonly record struct DocumentProductLine(
        string ProductCode,
        string ProductDescription,
        string Description,
        decimal UnitPrice,
        string TaxCode,
        decimal TaxPercentage);

    private DocumentProductLine[] GetDocumentLines()
    {
        var sourceDocs = saftValidator.SaftFile?.SourceDocuments;
        if (sourceDocs == null) return [];

        var invoiceLines = sourceDocs.SalesInvoices?.Invoice?
            .Where(i => i.Line != null)
            .SelectMany(i => i.Line)
            .Select(l => new DocumentProductLine(l.ProductCode, l.ProductDescription, l.Description, l.UnitPrice, l.Tax?.TaxCode, l.Tax?.TaxPercentage ?? 0m))
            ?? [];

        var movementLines = sourceDocs.MovementOfGoods?.StockMovement?
            .Where(m => m.Line != null)
            .SelectMany(m => m.Line)
            .Select(l => new DocumentProductLine(l.ProductCode, l.ProductDescription, l.Description, l.UnitPrice, l.Tax?.TaxCode, l.Tax?.TaxPercentage ?? 0m))
            ?? [];

        var workingLines = sourceDocs.WorkingDocuments?.WorkDocument?
            .Where(w => w.Line != null)
            .SelectMany(w => w.Line)
            .Select(l => new DocumentProductLine(l.ProductCode, l.ProductDescription, l.Description, l.UnitPrice, l.Tax?.TaxCode, l.Tax?.TaxPercentage ?? 0m))
            ?? [];

        return [.. invoiceLines, .. movementLines, .. workingLines];
    }

    private void ProcessProducts(IReadOnlyList<DocumentProductLine> documentLines)
    {
        foreach (var p in allProducts)
        {
            var productLines = documentLines
                .Where(l => l.ProductCode != null && string.Equals(l.ProductCode, p.ProductCode, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (string.IsNullOrWhiteSpace(p.ProductDescription))
            {
                p.ProductDescription = productLines.Select(x => x.ProductDescription).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d))
                    ?? productLines.Select(x => x.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d));
            }

            var prices = productLines
                .Select(l => l.UnitPrice.ToString("N3"))
                .Distinct();

            var pricesWithVat = productLines
                .Select(l => (l.UnitPrice * (1 + l.TaxPercentage / 100m)).ToString("N3"))
                .Distinct();

            var taxes = productLines
                .Select(l => l.TaxCode)
                .Where(t => t != null)
                .Distinct();

            p.Prices = string.Join(" | ", prices);
            p.PricesWithVat = string.Join(" | ", pricesWithVat);
            p.Taxes = string.Join(" | ", taxes);
        }
    }
}
