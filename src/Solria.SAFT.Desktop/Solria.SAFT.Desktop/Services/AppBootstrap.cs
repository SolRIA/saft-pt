using Microsoft.Extensions.DependencyInjection;
using System;

namespace SolRIA.SAFT.Desktop.Services;

public sealed class AppBootstrap
{
    private static IServiceProvider serviceProvider;
    public static void ConfigureServices(ServiceCollection services)
    {
        serviceProvider = services.BuildServiceProvider();
    }

    public static T Resolve<T>()
    {
        return serviceProvider.GetService<T>();
    }
}
