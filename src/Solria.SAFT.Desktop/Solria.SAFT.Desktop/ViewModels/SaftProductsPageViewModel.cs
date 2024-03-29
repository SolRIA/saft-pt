﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
        IsLoading = true;

        ToolTip = new ProductToolTipService();

        allProducts = saftValidator.SaftFile.MasterFiles.Product ?? [];

        if (allProducts.Any() == false) return;

        //calculated fields
        var invoices_lines = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line);
        if (invoices_lines != null)
        {
            foreach (var p in allProducts)
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

        Products = new List<Product>(allProducts);

        IsLoading = false;
    }

    [ObservableProperty]
    private ProductToolTipService toolTip;

    [ObservableProperty]
    private IList<Product> products;

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Products == null || Products.Count == 0) return;

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
                stringBuilder.AppendLine($"{c.ProductCode};{c.ProductDescription};;{c.Prices};{c.ProductNumberCode};{c.ProductGroup};{c.Taxes}");
            }

            File.WriteAllText(file, stringBuilder.ToString());
        }
    }

    [RelayCommand]
    private void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Filter))
        {
            Products = new List<Product>(allProducts);
            return;
        }

        Products = allProducts
            .Where(d => FilterEntries(d, Filter))
            .ToList();
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
}
