using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.Views;
using SolRIA.SAFT.Parser.Services;

namespace SolRIA.SAFT.Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();

                InitServices(mainWindow);

                desktop.MainWindow = mainWindow;

                mainWindow.NavigateToFirstPage();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void InitServices(MainWindow mainWindow)
        {
            var services = new ServiceCollection();

            services.AddSingleton<INavigationService>(new NavigationService());
            services.AddSingleton<ISaftValidator, SaftValidator>();
            services.AddSingleton<IDatabaseService, DatabaseService>();

            services.AddSingleton<IDialogManager>(mainWindow);
            services.AddSingleton<IReportService>(mainWindow);

            AppBootstrap.ConfigureServices(services);
        }
    }
}
