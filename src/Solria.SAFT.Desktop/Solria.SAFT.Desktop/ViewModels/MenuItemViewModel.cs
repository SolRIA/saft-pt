using System.Collections.Generic;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public System.Windows.Input.ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public IList<MenuItemViewModel> Items { get; set; }
    }
}
