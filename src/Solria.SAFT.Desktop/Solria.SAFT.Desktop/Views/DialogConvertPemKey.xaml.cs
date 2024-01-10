using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class DialogConvertPemKey : Window
    {
        public DialogConvertPemKey()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
    }
}
