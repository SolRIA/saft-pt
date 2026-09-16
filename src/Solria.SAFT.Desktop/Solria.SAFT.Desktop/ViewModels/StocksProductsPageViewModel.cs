using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Infrastructure;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class StocksProductsPageViewModel : ViewModelBase
{
    readonly ISaftValidator saftValidator;
    readonly IDialogManager dialogManager;

    public StocksProductsPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        if (saftValidator?.StockFile == null) return;

        Init();
    }

    private void Init()
    {
        IsLoading = true;

        Products = saftValidator.StockFile.Stock ?? [];

        if (Products.Length == 0) return;

        TotalQuantity = Products.Sum(p => p.ClosingStockQuantity);
        TotalValue = Products.Sum(p => p.ClosingStockQuantity * p.ClosingStockValue);
        NumberProducts = Products.Length;
    }

    [ObservableProperty]
    public partial Stock[] Products { get; set; }

    [ObservableProperty]
    public partial decimal TotalQuantity { get; set; }

    [ObservableProperty]
    public partial decimal TotalValue { get; set; }

    [ObservableProperty]
    public partial decimal NumberProducts { get; set; }

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Products == null || Products.Length == 0) return;

        var (_, stream) = await dialogManager.SaveFileDialog(
            "Guardar stocks",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Stocks.csv",
            ".csv");

        if (stream == null) return;

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("ProductCategory;ProductCode;ProductDescription;ProductNumberCode;ClosingStockQuantity;UnitOfMeasure");
        foreach (var c in Products)
        {
            stringBuilder.AppendLine(CultureInfo.CurrentCulture, $"""
                {c.ProductCategory};"{c.ProductCode}";"{c.ProductDescription}";"{c.ProductNumberCode}";{c.ClosingStockQuantity};{c.UnitOfMeasure}
                """);
                
        }

        await stream.Save(stringBuilder.ToString()).ConfigureAwait(false);
    }

    [RelayCommand]
    private void OnSearch()
    {
        var products = saftValidator.StockFile.Stock ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Products = products;

            return;
        }

        Products = products
            .Where(d => FilterProducts(d, Filter))
            .ToArray();
    }
    private static bool FilterProducts(Stock product, string filter)
    {
        if (string.IsNullOrWhiteSpace(product.ProductCode) == false && product.ProductCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(product.ProductDescription) == false && product.ProductDescription.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(product.ProductNumberCode) == false && product.ProductNumberCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(product.UnitOfMeasure) == false && product.UnitOfMeasure.Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    [RelayCommand]
    private void OnSearchClear()
    {
        Filter = null;
        OnSearch();
    }
}
