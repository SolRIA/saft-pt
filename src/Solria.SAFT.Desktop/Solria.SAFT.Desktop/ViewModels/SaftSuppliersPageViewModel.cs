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
    public class SaftSuppliersPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public SaftSuppliersPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_SUPPLIERS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            ToolTip = new SupplierToolTipService();

            CollectionView = new DataGridCollectionView(saftValidator?.SaftFile?.MasterFiles?.Supplier ?? Array.Empty<Supplier>())
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is Supplier supplier)
                    {
                        if (supplier.AccountID != null && supplier.AccountID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.CompanyName != null && supplier.CompanyName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.Contact != null && supplier.Contact.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.SupplierID != null && supplier.SupplierID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.SupplierTaxID != null && supplier.SupplierTaxID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.Email != null && supplier.Email.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.Fax != null && supplier.Fax.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.Telephone != null && supplier.Telephone.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (supplier.Website != null && supplier.Website.Contains(Filter, StringComparison.OrdinalIgnoreCase))
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
        }

        protected override void HandleDeactivation()
        {
        }

        private SupplierToolTipService toolTip;
        public SupplierToolTipService ToolTip
        {
            get => toolTip;
            set => this.RaiseAndSetIfChanged(ref toolTip, value);
        }

        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchClearCommand { get; }

        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<Supplier> suppliers)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar fornecedores",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Fornecedores.csv",
                    ".csv");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    StringBuilder moradas = new StringBuilder();
                    foreach (var c in suppliers)
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