using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Parser.Models;
using Splat;
using System.Reactive.Disposables;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftHeaderPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public SaftHeaderPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_HEADER_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Header = saftValidator?.SaftFile?.Header ?? new Header();

            ToolTip = new HeaderToolTipService();
        }

        private Header header;
        public Header Header
        {
            get => header;
            set => this.RaiseAndSetIfChanged(ref header, value);
        }

        private HeaderToolTipService toolTip;
        public HeaderToolTipService ToolTip
        {
            get => toolTip;
            set => this.RaiseAndSetIfChanged(ref toolTip, value);
        }
    }
}
