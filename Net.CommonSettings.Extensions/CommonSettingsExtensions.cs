using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Net.CommonSettings.Extensions;

public static class CommonSettingsExtensions
{

    public static IHostBuilder UseCommonSettings<TCommonSettings>(this IHostBuilder builder,
        string configSectionName = CommonSettingsOptions.DefaultConfigSectionName) where TCommonSettings : class
    {
        return builder.UseCommonSettings<TCommonSettings>(c =>
        {
            c.ConfigSectionName = configSectionName;
        });
    }

    public static IHostBuilder UseCommonSettings<TCommonSettings>(this IHostBuilder builder,
        Action<CommonSettingsOptions> configure) where TCommonSettings : class
    {
        CommonSettingsOptions options = new();
        configure(options);

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var env = ctx.HostingEnvironment.EnvironmentName;

            // The order is important - we insert more specific settings at the very beginning
            // and then move them forward by inserting base settings at the very beginning as well.
            if (ctx.HostingEnvironment.IsDevelopment())
                cfg.AddCommonSettingsFile("commonsettings.User.json", options.IsUserFileOptional, 0);
            cfg.AddCommonSettingsFile($"commonsettings.{env}.json", options.IsEnvironmentFileOptional, 0);
            cfg.AddCommonSettingsFile("commonsettings.json", options.IsMainFileOptional, 0);
        });

        builder.ConfigureServices((ctx, srv) =>
        {
            srv.AddCommonSettings<TCommonSettings>(options.ConfigSectionName);
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
        source.ResolveFileProvider();

        if(source.FileProvider == null && builder.Properties.TryGetValue("FileProvider", out var fileProvider)
            && fileProvider is PhysicalFileProvider physicalFp)
        {
            var isExists = physicalFp.GetFileInfo(filePath).Exists;
            if(!isExists)
            {
                filePath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.FullName, filePath);
                source.Path = filePath;
                source.ResolveFileProvider();
            }
        }

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
