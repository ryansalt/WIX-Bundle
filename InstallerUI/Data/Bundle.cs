using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallerUI.Data
{
    public class Bundle
    {
        public bool PerMachine;
        public string Name;
        public string LogVariable;
        public Package[] Packages;

        // Custom table extension for license file
        public string LicenseFileName;
    }
}
