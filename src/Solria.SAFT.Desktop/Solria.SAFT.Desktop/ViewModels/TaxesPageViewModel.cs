using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class TaxesPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public TaxesPageViewModel(IScreen screen) : base(screen, MenuIds.TAXES_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
        }

        protected override void HandleActivation()
        {
            IsLoading = true;

            Task.Run(() =>
            {
                var taxes = new List<TaxTableEntry>();
                if (saftValidator?.SaftFileV4?.MasterFiles?.TaxTable != null)
                {
                    var saft_taxes = saftValidator.SaftFileV4.MasterFiles.TaxTable;

                    foreach (var c in saft_taxes)
                    {
                        taxes.Add(new TaxTableEntry
                        {
                            Description = c.Description,
                            TaxAmount = c.ItemElementName == Models.SaftV4.ItemChoiceType2.TaxAmount ? c.Item : 0,
                            TaxPercentage = c.ItemElementName == Models.SaftV4.ItemChoiceType2.TaxPercentage ? c.Item : 0,
                            TaxCode = c.TaxCode,
                            TaxCountryRegion = c.TaxCountryRegion,
                            TaxType = c.TaxType.ToString(),
                            TaxExpirationDate = c.TaxExpirationDateSpecified ? c.TaxExpirationDate.ToString("yyyy-MM-dd") : null
                        });
                    }
                }
                else if (saftValidator?.SaftFileV3?.MasterFiles?.TaxTable != null)
                {
                    var saft_taxes = saftValidator.SaftFileV4.MasterFiles.TaxTable;

                    foreach (var c in saft_taxes)
                    {
                        taxes.Add(new TaxTableEntry
                        {
                            Description = c.Description,
                            TaxAmount = c.ItemElementName == Models.SaftV4.ItemChoiceType2.TaxAmount ? c.Item : 0,
                            TaxPercentage = c.ItemElementName == Models.SaftV4.ItemChoiceType2.TaxPercentage ? c.Item : 0,
                            TaxCode = c.TaxCode,
                            TaxCountryRegion = c.TaxCountryRegion,
                            TaxType = c.TaxType.ToString(),
                            TaxExpirationDate = c.TaxExpirationDateSpecified ? c.TaxExpirationDate.ToString("yyyy-MM-dd") : null
                        });
                    }
                }

                return taxes;
            }).ContinueWith(async t =>
            {
                var taxes = await t;

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
                            if (tax.TaxType != null && tax.TaxType.Contains(Filter, StringComparison.OrdinalIgnoreCase))
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
                    .InvokeCommand(SearchCommand);

                IsLoading = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
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