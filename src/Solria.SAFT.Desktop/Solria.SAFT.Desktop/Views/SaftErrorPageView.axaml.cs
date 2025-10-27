using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class SaftErrorPageView : UserControl
    {
        public SaftErrorPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
