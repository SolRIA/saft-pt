using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class SaftPaymentsPageView : UserControl
    {
        public SaftPaymentsPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
