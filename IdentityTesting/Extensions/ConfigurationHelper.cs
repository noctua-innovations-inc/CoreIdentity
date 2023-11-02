using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityTesting.Extensions;

internal static class ConfigurationHelper
{
    public static void AddConfiguration(this IServiceCollection serviceCollection)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
    }
}