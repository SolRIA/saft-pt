using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Solria.SAFT.Desktop.Views
{
    public class DialogSaftResume : Window
    {
        public DialogSaftResume()
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
