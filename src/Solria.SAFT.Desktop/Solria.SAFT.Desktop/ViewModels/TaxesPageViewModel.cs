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
    public class TaxesPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public TaxesPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_TAXES_PAGE)
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

                var taxes = saftValidator?.SaftFile?.MasterFiles?.TaxTable ?? Array.Empty<TaxTableEntry>();

                CollectionView = new DataGridCollectionView(taxes)
                {
                    Filter = o =>
                    {
                        if (string.IsNullOrWhiteSpace(Filter))
                            return true;

                        if (o is TaxTableEntry tax)
                        {
                            if (tax.Description != null && tax.Description.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (tax.TaxCode != null && tax.TaxCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (tax.TaxCountryRegion != null && tax.TaxCountryRegion.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (tax.TaxType.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (tax.TaxAmount != null && tax.TaxAmount.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (tax.TaxPercentage != null && tax.TaxPercentage.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }

                        return false;
                    }
                };

                CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("TaxType"));

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
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<TaxTableEntry> taxes)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Impostos",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Impostos.csv",
                    ".csv");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var c in taxes)
                    {
                        stringBuilder.AppendLine($"{c.TaxType};{c.Description};{c.TaxCode};{c.TaxCountryRegion};{c.TaxAmount};{c.TaxPercentage};{c.TaxExpirationDate}");
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