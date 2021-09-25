using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Solria.SAFT.Desktop.Views
{
    public class SaftDialogResume : Window
    {
        public SaftDialogResume()
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
