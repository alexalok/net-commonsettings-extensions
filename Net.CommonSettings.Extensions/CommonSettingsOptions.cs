using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.CommonSettings.Extensions;

public class CommonSettingsOptions
{
    public const string DefaultConfigSectionName = "Common";

    public string ConfigSectionName { get; set; } = DefaultConfigSectionName;
    public bool IsMainFileOptional { get; set; } = true;
    public bool IsEnvironmentFileOptional { get; set; } = true;
    public bool IsUserFileOptional { get; set; } = true;
}
