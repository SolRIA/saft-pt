using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class SaftCustomersPageView : UserControl
    {
        public SaftCustomersPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
