using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Stock;
using Solria.SAFT.Desktop.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class StocksProductsPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;
        readonly IDialogManager dialogManager;

        public StocksProductsPageViewModel(IScreen screen) : base(screen, MenuIds.STOCKS_PRODUCTS_PAGE)
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

            Task.Run(() =>
            {
                var products = new List<Stock>();
                if (saftValidator?.StockFile != null)
                {
                    var saft_products = saftValidator.StockFile.Stock;

                    //create binding products
                    foreach (var c in saft_products)
                    {
                        products.Add(new Stock
                        {
                            ProductCode = c.ProductCode,
                            ProductDescription = c.ProductDescription,
                            ProductNumberCode = c.ProductNumberCode,
                            ClosingStockQuantity = c.ClosingStockQuantity,
                            ProductCategory = c.ProductCategory.ToString(),
                            UnitOfMeasure = c.UnitOfMeasure
                        });
                    }
                }
                
                return products;
            }).ContinueWith(async p =>
            {
                var products = await p;

                CollectionView = new DataGridCollectionView(products)
                {
                    Filter = o =>
                    {
                        if (string.IsNullOrWhiteSpace(Filter))
                            return true;

                        if (o is Stock product)
                        {
                            if (product.ProductCode != null && product.ProductCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (product.ProductDescription != null && product.ProductDescription.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (product.ProductNumberCode != null && product.ProductNumberCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (product.ProductCategory != null && product.ProductCategory.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (product.UnitOfMeasure != null && product.UnitOfMeasure.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }

                        return false;
                    }
                };

                TotalQuantity = products.Sum(p => p.ClosingStockQuantity);
                TotalValue = products.Sum(p => p.ClosingStockQuantity * p.ClosingStockValue);
                NumberProducts = products.Count;

                CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("ProductCategory"));

                IsLoading = false;

                this.WhenAnyValue(x => x.Filter)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(disposables);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void HandleDeactivation()
        {

        }

        private decimal totalQuantity;
        public decimal TotalQuantity
        {
            get => totalQuantity;
            set => this.RaiseAndSetIfChanged(ref totalQuantity, value);
        }

        private decimal totalValue;
        public decimal TotalValue
        {
            get => totalValue;
            set => this.RaiseAndSetIfChanged(ref totalValue, value);
        }

        private decimal numberProducts;
        public decimal NumberProducts
        {
            get => numberProducts;
            set => this.RaiseAndSetIfChanged(ref numberProducts, value);
        }


        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchClearCommand { get; }

        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<Stock> products)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar produtos",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Produtos.csv",
                    ".csv");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var c in products)
                    {
                        stringBuilder.AppendLine($"{c.ProductCode};{c.ProductDescription};{c.ProductNumberCode};{c.ProductCategory};{c.UnitOfMeasure};{c.ClosingStockQuantity}");
                    }

                    File.WriteAllText(file, stringBuilder.ToString());
                }
            }
        }

        private void OnSearch(string _)
        {
            CollectionView?.Refresh();
        }
        private void OnSearchClear()
        {
            Filter = null;
        }
    }
}
