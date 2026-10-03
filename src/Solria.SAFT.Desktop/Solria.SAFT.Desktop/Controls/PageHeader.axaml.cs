using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using System.Windows.Input;

namespace SolRIA.SAFT.Desktop.Controls;

public partial class PageHeader : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PageHeader, string>(nameof(Title));
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<PageHeader, string>(nameof(Description));
    public static readonly StyledProperty<ICommand> DetachCommandProperty =
        AvaloniaProperty.Register<PageHeader, ICommand>(nameof(DetachCommand));
    public static readonly StyledProperty<bool> CanDetachProperty =
        AvaloniaProperty.Register<PageHeader, bool>(nameof(CanDetach), true);

    public string Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public ICommand DetachCommand { get => GetValue(DetachCommandProperty); set => SetValue(DetachCommandProperty, value); }
    public bool CanDetach { get => GetValue(CanDetachProperty); set => SetValue(CanDetachProperty, value); }

    public PageHeader()
    {
        InitializeComponent();
        DetachCommand = new RelayCommand(() => AppBootstrap.Resolve<INavigationService>()?.DetachCurrentPage());
    }
}
