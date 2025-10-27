using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class StocksProductsPageView : UserControl
    {
        public StocksProductsPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
