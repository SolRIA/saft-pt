using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Solria.SAFT.Desktop.ViewModels;

namespace Solria.SAFT.Desktop.Views
{
    public class StocksProductsPageView : ReactiveUserControl<StocksProductsPageViewModel>
    {
        public StocksProductsPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
