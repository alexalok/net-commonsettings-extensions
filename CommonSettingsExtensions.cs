using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Net.CommonSettings.Extensions
{
    public static class CommonSettingsExtensions
    {
        public static IHostBuilder AddCommonSettings<TCommonSettings>(this IHostBuilder builder, bool isLoadUserSettings) where TCommonSettings : class
        {
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                var env = ctx.HostingEnvironment.EnvironmentName;
                cfg.AddCommonSettings("commonSettings.json", optional: false);
                cfg.AddCommonSettings($"commonSettings.{env}.json", optional: true);
                if (isLoadUserSettings)
                    cfg.AddCommonSettings("commonSettings.User.json", optional: true);
            });

            builder.ConfigureServices((_, srv) =>
            {
                srv.AddCommonSettings<TCommonSettings>();
            });

            return builder;
        }

        static IConfigurationBuilder AddCommonSettings(this IConfigurationBuilder builder,
            string filePath, bool optional)
        {
            // Workaround for WebSDK projects when launched via VS.
            // See https://github.com/dotnet/project-system/issues/5053.
            if (!Path.IsPathFullyQualified(filePath) && !File.Exists(filePath))
                filePath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.FullName, filePath);

            return builder.AddJsonFile(filePath, optional);
        }

        static IServiceCollection AddCommonSettings<TCommonSettings>(this IServiceCollection srv) where TCommonSettings : class
        {
            return srv.AddOptions<TCommonSettings>()
                .BindConfiguration("Common")
                .Services;
        }
    }
}
