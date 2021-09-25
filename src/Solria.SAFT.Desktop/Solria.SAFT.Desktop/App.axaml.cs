using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.ViewModels;
using Solria.SAFT.Desktop.Views;
using Splat;
using System.Reflection;

namespace Solria.SAFT.Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();

                IXmlSerializer xmlSerializer = new XmlSerializer();
                ISaftValidator saftValidator = new SaftValidator(xmlSerializer);
                IDatabaseService databaseService = new DatabaseService();
                IReportService reportService = mainWindow;

                Locator.CurrentMutable.RegisterConstant(saftValidator, typeof(ISaftValidator));
                Locator.CurrentMutable.RegisterConstant(databaseService, typeof(IDatabaseService));
                Locator.CurrentMutable.RegisterConstant(mainWindow, typeof(IDialogManager));
                Locator.CurrentMutable.RegisterConstant(reportService, typeof(IReportService));

                mainWindow.DataContext = new MainWindowViewModel();
                desktop.MainWindow = mainWindow;

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
