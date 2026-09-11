using Avalonia.Controls;
using Avalonia.Interactivity;
using SolRIA.SAFT.Desktop.ViewModels;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class MainMenuPageView : UserControl
    {
        public MainMenuPageView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (Design.IsDesignMode) return;
            if (DataContext is not MainWindowViewModel vm) return;

            vm.Init();
        }
    }
}
