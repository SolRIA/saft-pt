using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Parser.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftCustomersPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public SaftCustomersPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_CUSTOMERS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            IsLoading = true;

            CollectionView = new DataGridCollectionView(saftValidator.SaftFile?.MasterFiles?.Customer ?? new Customer[] { })
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is Customer customer)
                    {
                        if (customer.AccountID != null && customer.AccountID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CompanyName != null && customer.CompanyName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Contact != null && customer.Contact.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CustomerID != null && customer.CustomerID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.CustomerTaxID != null && customer.CustomerTaxID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Email != null && customer.Email.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Fax != null && customer.Fax.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Telephone != null && customer.Telephone.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (customer.Website != null && customer.Website.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };

            CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("AccountID"));

            this.WhenAnyValue(x => x.Filter)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(disposables);

            IsLoading = false;

        }

        protected override void HandleDeactivation()
        {
        }

        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchClearCommand { get; }

        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<Customer> customers)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar clientes",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Clientes.csv",
                    ".csv");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    StringBuilder moradas = new StringBuilder();
                    foreach (var c in customers)
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
        }

        private void OnSearch(string _)
        {
            CollectionView.Refresh();
        }
        private void OnSearchClear()
        {
            Filter = null;
        }
    }
}