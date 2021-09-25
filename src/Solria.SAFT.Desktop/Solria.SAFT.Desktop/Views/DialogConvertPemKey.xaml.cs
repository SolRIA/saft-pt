using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Solria.SAFT.Desktop.Views
{
    public class DialogConvertPemKey : Window
    {
        public DialogConvertPemKey()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
