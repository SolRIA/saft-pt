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
        var invoices_lines = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line);
        if (invoices_lines != null)
        {
            ProcessProducts(invoices_lines);
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

        var (_, stream) = await dialogManager.SaveFileDialog(
            title: "Guardar produtos excel",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Produtos.csv",
            defaultExtension: ".csv");

        if (stream is null) return;

        var stringBuilder = new StringBuilder("Código;Descrição;Preço;Código de Barras;Grupo;IVA;Preço com IVA");
        stringBuilder.AppendLine();
        foreach (var c in Products)
        {
            stringBuilder.AppendLine($"{c.ProductCode};{c.ProductDescription};{c.Prices};{c.ProductNumberCode};{c.ProductGroup};{c.Taxes};{c.PricesWithVat}");
        }

        await stream.Save(stringBuilder.ToString()).ConfigureAwait(false);
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
        var invoices_lines = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line);
        if (invoices_lines == null) return;


        var lineProducts = invoices_lines
            .Where(l => l.ProductCode != null)
            .Select(l => new Product
            {
                ProductCode = l.ProductCode,
                ProductDescription = l.ProductDescription,
                Prices = l.UnitPrice.ToString("N3"),
                ProductType = ProductType.P
            })
            .Distinct()
            .ToArray();

        var uniqueProducts = new List<Product>();
        foreach (var p in lineProducts)
        {
            if (uniqueProducts.Any(u => u.ProductCode.Equals(p.ProductCode, StringComparison.OrdinalIgnoreCase)))
                continue;

            uniqueProducts.Add(p);
        }

        allProducts = [.. uniqueProducts];
        ProcessProducts(invoices_lines);

        Products = [.. allProducts];
    }

    private void ProcessProducts(IEnumerable<SourceDocumentsSalesInvoicesInvoiceLine> invoices_lines)
    {
        foreach (var p in allProducts)
        {
            var productLines = invoices_lines
                .Where(l => l.ProductCode != null && string.Equals(l.ProductCode, p.ProductCode, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var prices = productLines
                .Select(l => l.UnitPrice.ToString("N3"))
                .Distinct();

            var pricesWithVat = productLines
                .Select(l => (l.UnitPrice * (1 + (l.Tax?.TaxPercentage ?? 0) / 100m)).ToString("N3"))
                .Distinct();

            var taxes = productLines
                .Select(l => l.Tax?.TaxCode)
                .Where(t => t != null)
                .Distinct();

            p.Prices = string.Join(" | ", prices);
            p.PricesWithVat = string.Join(" | ", pricesWithVat);
            p.Taxes = string.Join(" | ", taxes);
        }
    }
}
