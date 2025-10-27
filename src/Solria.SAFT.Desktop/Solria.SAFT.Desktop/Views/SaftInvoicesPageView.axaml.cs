using Avalonia.Controls;
using SolRIA.SAFT.Desktop.ViewModels;

namespace SolRIA.SAFT.Desktop.Views
{
    public static class DesignDataDialogSaftInvoices
    {
        public static SaftInvoicesPageViewModel ExampleViewModel { get; } =
            new SaftInvoicesPageViewModel
            {
                
            };
    }

    public partial class SaftInvoicesPageView : UserControl
    {
        public SaftInvoicesPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
