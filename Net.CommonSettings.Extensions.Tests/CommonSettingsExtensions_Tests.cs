using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Net.CommonSettings.Extensions.Tests;

public class CommonSettingsExtensions_Tests
{
    [Fact]
    public void Ensure_Order()
    {
        CommonSettings expectedCommandSettings =
            new()
            {
                AppOption = nameof(CommonSettings.AppOption),
                BaseOption = nameof(CommonSettings.BaseOption),
                CommandLineOption = nameof(CommonSettings.CommandLineOption),
                OverriddenOption = nameof(CommonSettings.OverriddenOption)
            };

        // Arrange & Act
        string[] args = { "Common:CommandLineOption=CommandLineOption" };
        IHost host = Host.CreateDefaultBuilder(args)
            .UseCommonSettings<CommonSettings>()
            .Build();

        CommonSettings commonSettings = host.Services.GetRequiredService<IOptions<CommonSettings>>().Value;

        // Assert
        commonSettings.Should()
            .BeEquivalentTo(expectedCommandSettings);
    }

    [Fact]
    public void Ensure_Custom_ConfigSectionName()
    {
        const string customSectionName = "CustomSectionName";
        CommonSettings expected = new()
        {
            BaseOption = customSectionName + nameof(CommonSettings.BaseOption)
        };

        // Arrange & Act
        IHost host = Host.CreateDefaultBuilder()
            .UseCommonSettings<CommonSettings>(customSectionName)
            .Build();

        CommonSettings commonSettings = host.Services.GetRequiredService<IOptions<CommonSettings>>().Value;

        // Assert
        commonSettings.Should()
            .BeEquivalentTo(expected);
    }

    [Fact]
    public void Ensure_User_Config_Is_Used_In_Development()
    {
        // Arrange & Act
        string[] args = { "environment=Development" };
        IHost host = Host.CreateDefaultBuilder(args)
            .UseCommonSettings<CommonSettings>()
            .Build();

        CommonSettings commonSettings = host.Services.GetRequiredService<IOptions<CommonSettings>>().Value;

        // Assert
        commonSettings.BaseOption.Should().BeEquivalentTo("UserOption");
    }

    [Fact]
    public void Ensure_User_Config_Is__Not_Used_In_Non_Development()
    {
        // Arrange & Act
        IHost host = Host.CreateDefaultBuilder()
            .UseCommonSettings<CommonSettings>()
            .Build();

        CommonSettings commonSettings = host.Services.GetRequiredService<IOptions<CommonSettings>>().Value;

        // Assert
        commonSettings.BaseOption.Should().BeEquivalentTo("BaseOption");
    }

    [Fact]
    public void Ensure_Config_File_Reads_From_Absolute_Path()
    {
        // Arrange & Act
        string[] args = { "environment=Development" };
        IHost host = Host.CreateDefaultBuilder(args)
            .UseCommonSettings<CommonSettings>(opt =>
            {
                opt.IsEnvironmentFileOptional = false;
            })
            .Build();

        CommonSettings commonSettings = host.Services.GetRequiredService<IOptions<CommonSettings>>().Value;

        // Assert
        commonSettings.OverriddenOption.Should().BeEquivalentTo("HelperFile");
    }

    [Fact]
    public void Ensure_Config_File_Reads_From_Relative_Path_When_DefaultFileProvider_Points_To_Another_Directory()
    {
        // Arrange & Act
        string[] args = { "environment=Development" };
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(cfg =>
            {
                var anotherDir = Path.GetTempPath();
                cfg.Properties["FileProvider"] = new PhysicalFileProvider(anotherDir);
            })
            .UseCommonSettings<CommonSettings>(opt =>
            {
                opt.IsEnvironmentFileOptional = false;
            })
            .Build();
        CommonSettings commonSettings = host.Services.GetRequiredService<IOptions<CommonSettings>>().Value;
        // Assert
        commonSettings.OverriddenOption.Should().BeEquivalentTo("HelperFile");
    }
}