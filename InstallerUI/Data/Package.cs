using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallerUI.Data
{
    public class Package
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public PackageType Type;
        public bool Permanent;
        public bool Vital;
        public bool DisplayInternalUI;

        //not available until WiX 3.9.421.0
        public string ProductCode;
        public string UpgradeCode;
        public string Version;
    }
}
