using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Solria.SAFT.Desktop.ViewModels;

namespace Solria.SAFT.Desktop.Views
{
    public class ProductsPageView : ReactiveUserControl<ProductsPageViewModel>
    {
        public ProductsPageView()
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
