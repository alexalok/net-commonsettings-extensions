using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Net.CommonSettings.Extensions;

public static class CommonSettingsExtensions
{
    const string DefaultConfigSectionName = "Common";

    public static IHostBuilder UseCommonSettings<TCommonSettings>(this IHostBuilder builder,
        string configSectionName = DefaultConfigSectionName) where TCommonSettings : class
    {
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var env = ctx.HostingEnvironment.EnvironmentName;

            // The order is important - we insert more specific settings at the very beginning
            // and then move them forward by inserting base settings at the very beginning as well.
            if (ctx.HostingEnvironment.IsDevelopment())
                cfg.AddCommonSettingsFile("commonsettings.User.json", true, 0);
            cfg.AddCommonSettingsFile($"commonsettings.{env}.json", true, 0);
            cfg.AddCommonSettingsFile("commonsettings.json", true, 0);
        });

        builder.ConfigureServices((ctx, srv) =>
        {
            srv.AddCommonSettings<TCommonSettings>(configSectionName);
        });

        return builder;
    }

    static IConfigurationBuilder AddCommonSettingsFile(this IConfigurationBuilder builder,
        string filePath, bool optional, int insertAt)
    {
        // Workaround for WebSDK projects when launched via VS.
        // See https://github.com/dotnet/project-system/issues/5053.
        if (!Path.IsPathFullyQualified(filePath) && !File.Exists(filePath))
            filePath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.FullName, filePath);

        JsonConfigurationSource source = new() { Path = filePath, Optional = optional };

        builder.Sources.Insert(insertAt, source);
        return builder;
    }

    static IServiceCollection AddCommonSettings<TCommonSettings>(this IServiceCollection srv,
        string? configSectionName) where TCommonSettings : class
    {
        return srv.AddOptions<TCommonSettings>()
            .BindConfiguration(configSectionName)
            .Services;
    }
}
