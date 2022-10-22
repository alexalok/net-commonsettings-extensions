using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.CommonSettings.Extensions.Tests
{
    public class CommonSettings
    {
        public string AppOption { get; set; } = null!;
        public string BaseOption { get; set; } = null!;
        public string OverriddenOption { get; set; } = null!;
        public string CommandLineOption { get; set; } = null!;
    }
}
