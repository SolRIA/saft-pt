using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Solria.SAFT.Desktop.ViewModels;

namespace Solria.SAFT.Desktop.Views
{
    public class WorkingDocumentsPageView : ReactiveUserControl<WorkingDocumentsPageViewModel>
    {
        public WorkingDocumentsPageView()
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
