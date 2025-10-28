using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftTaxesPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftTaxesPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        Taxes = saftValidator?.SaftFile?.MasterFiles?.TaxTable ?? [];
    }

    [ObservableProperty]
    private IList<TaxTableEntry> taxes;

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Taxes == null || Taxes.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar Impostos",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Impostos.csv",
            ".csv");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var c in Taxes)
            {
                stringBuilder.AppendLine($"{c.TaxType};{c.Description};{c.TaxCode};{c.TaxCountryRegion};{c.TaxAmount};{c.TaxPercentage};{c.TaxExpirationDate}");
            }

            File.WriteAllText(file, stringBuilder.ToString());
        }
    }

    [RelayCommand]
    private void OnSearch()
    {
        var taxes = saftValidator?.SaftFile?.MasterFiles?.TaxTable ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Taxes = taxes;

            return;
        }

        Taxes = taxes
            .Where(d => FilterEntries(d, Filter))
            .ToArray();
    }
    private static bool FilterEntries(TaxTableEntry entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.Description) == false && entry.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(entry.TaxCode) == false && entry.TaxCode.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.TaxCountryRegion) == false && entry.TaxCountryRegion.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.TaxType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.TaxAmount != null && entry.TaxAmount.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.TaxPercentage != null && entry.TaxPercentage.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
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