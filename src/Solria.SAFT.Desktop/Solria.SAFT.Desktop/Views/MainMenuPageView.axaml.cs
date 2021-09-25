using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Solria.SAFT.Desktop.Views
{
    public class MainMenuPageView : UserControl
    {
        public MainMenuPageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
