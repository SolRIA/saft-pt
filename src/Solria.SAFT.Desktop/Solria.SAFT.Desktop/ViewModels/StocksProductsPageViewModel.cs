using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
    private Stock[] products;

    [ObservableProperty]
    private decimal totalQuantity;

    [ObservableProperty]
    private decimal totalValue;

    [ObservableProperty]
    private decimal numberProducts;

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Products == null || Products.Length == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar produtos",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Produtos.csv",
            ".csv");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var c in Products)
            {
                stringBuilder.AppendLine($"{c.ProductCode};{c.ProductDescription};{c.ProductNumberCode};{c.ProductCategory};{c.UnitOfMeasure};{c.ClosingStockQuantity}");
            }

            File.WriteAllText(file, stringBuilder.ToString());
        }
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
