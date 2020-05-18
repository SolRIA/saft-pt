using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Stock;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Reactive.Disposables;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class StocksHeaderPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public StocksHeaderPageViewModel(IScreen screen) : base(screen, MenuIds.STOCKS_HEADER_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            if (saftValidator?.StockFile?.StockHeader != null)
            {
                var h = saftValidator.StockFile.StockHeader;
                Header = new StockHeader
                {
                    EndDate = h.EndDate,
                    FileVersion = h.FileVersion,
                    FiscalYear = h.FiscalYear,
                    NoStock = h.NoStock,
                    TaxRegistrationNumber = h.TaxRegistrationNumber
                };
            }
        }

        private StockHeader header;
        public StockHeader Header
        {
            get => header;
            set => this.RaiseAndSetIfChanged(ref header, value);
        }
    }
}
