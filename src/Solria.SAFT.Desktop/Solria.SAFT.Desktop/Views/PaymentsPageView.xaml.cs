using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Solria.SAFT.Desktop.ViewModels;

namespace Solria.SAFT.Desktop.Views
{
    public class PaymentsPageView : ReactiveUserControl<PaymentsPageViewModel>
    {
        public PaymentsPageView()
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
