using CommunityToolkit.Mvvm.ComponentModel;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class StocksHeaderPageViewModel : ViewModelBase
{
    readonly ISaftValidator saftValidator;

    public StocksHeaderPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();

        if (saftValidator?.StockFile?.StockHeader == null) return;

        Init();
    }

    private void Init()
    {
        Header = saftValidator.StockFile.StockHeader;
    }

    [ObservableProperty]
    private StockHeader header;
}
