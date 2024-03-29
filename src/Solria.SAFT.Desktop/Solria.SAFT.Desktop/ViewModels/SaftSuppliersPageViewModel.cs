﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftSuppliersPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftSuppliersPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        ToolTip = new SupplierToolTipService();

        Suppliers = saftValidator?.SaftFile?.MasterFiles?.Supplier ?? [];
    }

    [ObservableProperty]
    private SupplierToolTipService toolTip;

    [ObservableProperty]
    private Supplier[] suppliers;

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Suppliers == null || Suppliers.Length == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar fornecedores",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Fornecedores.csv",
            ".csv");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder moradas = new StringBuilder();
            foreach (var c in Suppliers)
            {
                moradas.Clear();
                if (c.BillingAddress != null)
                {
                    moradas.Append(
                        c.BillingAddress.AddressDetail ??
                        c.BillingAddress.StreetName + " " + c.BillingAddress.BuildingNumber + " " + c.BillingAddress.PostalCode);
                }

                stringBuilder.AppendLine($"{c.SupplierTaxID};{c.CompanyName};{c.SupplierID};{moradas};{c.Telephone};;{c.Fax};{c.Email}");
            }

            File.WriteAllText(file, stringBuilder.ToString());
        }
    }

    [RelayCommand]
    private void OnSearch()
    {
        var suppliers = saftValidator?.SaftFile?.MasterFiles?.Supplier ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Suppliers = suppliers;

            return;
        }

        Suppliers = suppliers
            .Where(d => FilterEntries(d, Filter))
            .ToArray();
    }
    private static bool FilterEntries(Supplier entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.AccountID) == false && entry.AccountID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.CompanyName) == false && entry.CompanyName.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Contact) == false && entry.Contact.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.SupplierID) == false && entry.SupplierID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.SupplierTaxID) == false && entry.SupplierTaxID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Email) == false && entry.Email.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Fax) == false && entry.Fax.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Telephone) == false && entry.Telephone.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Website) == false && entry.Website.Contains(filter, StringComparison.OrdinalIgnoreCase))
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