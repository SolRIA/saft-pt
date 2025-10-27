using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class SaftProductsPageView : UserControl
    {
        public SaftProductsPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
