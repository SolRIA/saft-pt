using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class DialogMessageDesignViewModel : DialogMessageViewModel
{
    public DialogMessageDesignViewModel() : base(MessageDialogType.Warning)
    {
        Title = "Título";
        Message = "Mensagem a apresentar";
    }
}

public partial class DialogMessageViewModel : ViewModelBase
{
    public DialogMessageViewModel(MessageDialogType messageDialogType)
    {
        switch (messageDialogType)
        {
            case MessageDialogType.None:
                break;
            case MessageDialogType.Information:
                Icon = Material.Icons.MaterialIconKind.Information;
                Color = Brush.Parse("#42A5F5");
                break;
            case MessageDialogType.Warning:
                Icon = Material.Icons.MaterialIconKind.Warning;
                Color = Brush.Parse("#FDD835");
                break;
            case MessageDialogType.Error:
                Icon = Material.Icons.MaterialIconKind.Error;
                Color = Brush.Parse("#F4511E");
                break;
            case MessageDialogType.Question:
                Icon = Material.Icons.MaterialIconKind.QuestionMark;
                Color = Brush.Parse("#FF9800");
                break;
            case MessageDialogType.Success:
                Icon = Material.Icons.MaterialIconKind.CheckboxMarked;
                Color = Brush.Parse("#43A047");
                break;
            default:
                break;
        }
    }

    [ObservableProperty]
    public partial string Message { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial Material.Icons.MaterialIconKind Icon { get; set; }

    [ObservableProperty]
    public partial IBrush Color { get; set; }

    [RelayCommand]
    private void OnOk()
    {
    }

    [RelayCommand]
    private void OnCancel()
    {
    }
}
