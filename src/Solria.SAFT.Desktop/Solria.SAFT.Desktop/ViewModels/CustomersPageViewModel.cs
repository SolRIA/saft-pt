using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Collections.Generic;
using System.Reactive;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class CustomersPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public CustomersPageViewModel(IScreen screen) : base(screen, MenuIds.ERRORS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();

            DoPrintCommand = ReactiveCommand.Create(OnDoPrint);
            GenerateScriptCommand = ReactiveCommand.Create(OnGenerateScript);
            SearchCommand = ReactiveCommand.Create(OnSearch);
        }

        protected override void HandleActivation()
        {
            var customers = new List<Customer>();
            if (saftValidator?.SaftFileV4?.MasterFiles?.Customer != null)
            {
                var saft_customers = saftValidator.SaftFileV4.MasterFiles.Customer;

                foreach (var c in saft_customers)
                {
                    customers.Add(new Customer
                    {
                        AccountID = c.AccountID,
                        BillingAddress = new AddressStructure
                        {
                            AddressDetail = c.BillingAddress?.AddressDetail,
                            BuildingNumber = c.BillingAddress?.BuildingNumber,
                            City = c.BillingAddress?.City,
                            Country = c.BillingAddress?.Country,
                            PostalCode = c.BillingAddress?.PostalCode,
                            Region = c.BillingAddress?.Region,
                            StreetName = c.BillingAddress?.StreetName
                        },
                        CompanyName = c.CompanyName,
                        Contact = c.Contact,
                        CustomerID = c.CustomerID,
                        CustomerTaxID = c.CustomerTaxID,
                        Email = c.Email,
                        Fax = c.Fax,
                        SelfBillingIndicator = c.SelfBillingIndicator,
                        Telephone = c.Telephone,
                        Website = c.Website
                    });
                }
            }
            else if (saftValidator?.SaftFileV3?.MasterFiles?.Customer != null)
            {
                var saft_customers = saftValidator.SaftFileV3.MasterFiles.Customer;

                foreach (var c in saft_customers)
                {
                    customers.Add(new Customer
                    {
                        AccountID = c.AccountID,
                        BillingAddress = new AddressStructure
                        {
                            AddressDetail = c.BillingAddress?.AddressDetail,
                            BuildingNumber = c.BillingAddress?.BuildingNumber,
                            City = c.BillingAddress?.City,
                            Country = c.BillingAddress?.Country,
                            PostalCode = c.BillingAddress?.PostalCode,
                            Region = c.BillingAddress?.Region,
                            StreetName = c.BillingAddress?.StreetName
                        },
                        CompanyName = c.CompanyName,
                        Contact = c.Contact,
                        CustomerID = c.CustomerID,
                        CustomerTaxID = c.CustomerTaxID,
                        Email = c.Email,
                        Fax = c.Fax,
                        SelfBillingIndicator = c.SelfBillingIndicator,
                        Telephone = c.Telephone,
                        Website = c.Website
                    });
                }
            }

            CollectionView = new DataGridCollectionView(customers)
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is Customer customer)
                    {
                        if (customer.AccountID != null && customer.AccountID.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CompanyName != null && customer.CompanyName.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Contact != null && customer.Contact.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CustomerID != null && customer.CustomerID.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CustomerTaxID != null && customer.CustomerTaxID.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CustomerTaxID != null && customer.CustomerTaxID.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Email != null && customer.Email.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Fax != null && customer.Fax.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };
            CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("AccountID"));
        }

        protected override void HandleDeactivation()
        {

        }

        private string filter;
        public string Filter
        {
            get => filter;
            set => this.RaiseAndSetIfChanged(ref filter, value);
        }

        private DataGridCollectionView collectionView;
        public DataGridCollectionView CollectionView
        {
            get => collectionView;
            set => this.RaiseAndSetIfChanged(ref collectionView, value);
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
