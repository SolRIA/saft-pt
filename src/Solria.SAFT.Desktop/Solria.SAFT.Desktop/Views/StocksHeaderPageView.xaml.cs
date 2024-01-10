using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class StocksHeaderPageView : UserControl
    {
        public StocksHeaderPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
