using Avalonia.Controls;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class SaftHeaderPageView : UserControl
    {
        public SaftHeaderPageView()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
