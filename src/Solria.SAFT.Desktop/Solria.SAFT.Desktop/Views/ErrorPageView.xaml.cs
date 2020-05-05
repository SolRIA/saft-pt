using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Solria.SAFT.Desktop.ViewModels;

namespace Solria.SAFT.Desktop.Views
{
    public class ErrorPageView : ReactiveUserControl<ErrorPageViewModel>
    {
        public ErrorPageView()
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
