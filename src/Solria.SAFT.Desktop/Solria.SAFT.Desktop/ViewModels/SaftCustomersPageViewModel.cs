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

public partial class SaftCustomersPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftCustomersPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        List = saftValidator.SaftFile?.MasterFiles?.Customer ?? [];
    }

    [ObservableProperty]
    private IList<Customer> list;

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (List == null || List.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar clientes",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Clientes.csv",
            ".csv");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder moradas = new StringBuilder();
            foreach (var c in List)
            {
                moradas.Clear();
                if (c.BillingAddress != null)
                {
                    moradas.Append(
                        c.BillingAddress.AddressDetail ??
                        c.BillingAddress.StreetName + " " + c.BillingAddress.BuildingNumber + " " + c.BillingAddress.PostalCode);
                }

                stringBuilder.AppendLine($"{c.CustomerTaxID};{c.CompanyName};{c.CustomerID};{moradas};{c.Telephone};;{c.Fax};{c.Email}");
            }

            File.WriteAllText(file, stringBuilder.ToString());
        }
    }

    [RelayCommand]
    private void OnSearch()
    {
        var customers = saftValidator.SaftFile?.MasterFiles?.Customer ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            List = new List<Customer>(customers);

            return;
        }

        List = customers
            .Where(d => FilterEntries(d, Filter))
            .ToArray();
    }
    private static bool FilterEntries(Customer entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.AccountID) == false && entry.AccountID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.CompanyName) == false && entry.CompanyName.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Contact) == false && entry.Contact.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.CustomerID) == false && entry.CustomerID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.CustomerTaxID) == false && entry.CustomerTaxID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
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