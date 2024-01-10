using CommunityToolkit.Mvvm.ComponentModel;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftHeaderPageViewModel : ViewModelBase
{
    readonly ISaftValidator saftValidator;

    public SaftHeaderPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        Init();
    }

    private void Init()
    {
        Header = saftValidator?.SaftFile?.Header ?? new Header();

        ToolTip = new HeaderToolTipService();
    }

    [ObservableProperty]
    private Header header;
    
    [ObservableProperty]
    private HeaderToolTipService toolTip;
}
