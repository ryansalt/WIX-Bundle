using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace InstallerUI.Data
{
    public class BootstrapperApplicationData
    {
        public const string defaultFileName = "BootstrapperApplicationData.xml";
        public const string xmlNamespace = "http://schemas.microsoft.com/wix/2010/BootstrapperApplicationData";

        private static string defaultFolder;
        public static string DefaultFolder
        {
            get
            {
                if (defaultFolder == null)
                {
                    defaultFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                return defaultFolder;
            }
        }

        private static string defaultFile;
        public static string DefaultFile
        {
            get
            {
                if (defaultFile == null)
                {
                    defaultFile = Path.Combine(DefaultFolder, defaultFileName);
                }

                return defaultFile;
            }
        }

        public FileInfo DataFile { get; protected set; }
        public Bundle Data { get; protected set; }

        public BootstrapperApplicationData() : this(DefaultFile) { }

        public BootstrapperApplicationData(string bootstrapperApplicationDataFile)
        {
            using (FileStream fs = File.OpenRead(bootstrapperApplicationDataFile))
            {
                Data = ParseBundleFromStream(fs);
            }
        }

        public static Bundle ParseBundleFromStream(Stream stream)
        {
            XPathDocument manifest = new XPathDocument(stream);
            XPathNavigator root = manifest.CreateNavigator();
            return ParseBundleFromXml(root);
        }

        public static Bundle ParseBundleFromXml(XPathNavigator root)
        {
            Bundle bundle = new Bundle();

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(root.NameTable);
            namespaceManager.AddNamespace("p", xmlNamespace);
            XPathNavigator bundleNode = root.SelectSingleNode("/p:BootstrapperApplicationData/p:WixBundleProperties", namespaceManager);

            if (bundleNode == null)
            {
                throw new Exception("Failed to select bundle information");
            }

            bool? perMachine = GetYesNoAttribute(bundleNode, "PerMachine");
            if (perMachine.HasValue)
            {
                bundle.PerMachine = perMachine.Value;
            }

            string name = GetAttribute(bundleNode, "DisplayName");
            if (name != null)
            {
                bundle.Name = name;
            }

            string logVariable = GetAttribute(bundleNode, "LogPathVariable");
            if (logVariable != null)
            {
                bundle.LogVariable = logVariable;
            }
            else
            {
                // Wix would actually debug "Failed to select bundle information" and return with E_NOTFOUND, but I think it's a (harmless) bug
            }

            Package[] packages = ParsePackagesFromXml(root);
            bundle.Packages = packages;

            // Custom table extension for license file
            XPathNavigator licenseNode = root.SelectSingleNode("/p:BootstrapperApplicationData/p:LicenseInformation", namespaceManager);
            if (licenseNode != null)
            {
                bundle.LicenseFileName = GetAttribute(licenseNode, "LicenseFile");
            }

            return bundle;
        }

        public static Package[] ParsePackagesFromXml(XPathNavigator root)
        {
            List<Package> packages = new List<Package>();

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(root.NameTable);
            namespaceManager.AddNamespace("p", xmlNamespace);
            XPathNodeIterator nodes = root.Select("/p:BootstrapperApplicationData/p:WixPackageProperties", namespaceManager);

            foreach (XPathNavigator node in nodes)
            {
                Package package = new Package();

                string id = GetAttribute(node, "Package");
                package.Id = id ?? throw new Exception("Failed to get package identifier for package");

                string displayName = GetAttribute(node, "DisplayName");
                if (displayName != null)
                {
                    package.DisplayName = displayName;
                }

                string description = GetAttribute(node, "Description");
                if (description != null)
                {
                    package.Description = description;
                }

                PackageType? packageType = GetPackageTypeAttribute(node, "PackageType");
                if (!packageType.HasValue)
                {
                    throw new Exception("Failed to get package type for package");
                }
                package.Type = packageType.Value;

                bool? permanent = GetYesNoAttribute(node, "Permanent");
                if (!permanent.HasValue)
                {
                    throw new Exception("Failed to get permanent settings for package");
                }
                package.Permanent = permanent.Value;

                bool? vital = GetYesNoAttribute(node, "Vital");
                if (!vital.HasValue)
                {
                    throw new Exception("Failed to get vital setting for package");
                }
                package.Vital = vital.Value;

                bool? displayInternalUI = GetYesNoAttribute(node, "DisplayInternalUI");
                if (!displayInternalUI.HasValue)
                {
                    throw new Exception("Failed to get DisplayInternalUI setting for package");
                }
                package.DisplayInternalUI = displayInternalUI.Value;

                string productCode = GetAttribute(node, "ProductCode");
                if (productCode != null)
                {
                    package.ProductCode = productCode;
                }

                string upgradeCode = GetAttribute(node, "UpgradeCode");
                if (upgradeCode != null)
                {
                    package.UpgradeCode = upgradeCode;
                }

                string version = GetAttribute(node, "Version");
                if (version != null)
                {
                    package.Version = version;
                }

                packages.Add(package);
            }

            return packages.ToArray();
        }

        public static string GetAttribute(XPathNavigator node, string attributeName)
        {
            XPathNavigator attribute = node.SelectSingleNode("@" + attributeName);

            if (attribute == null)
            {
                return null;
            }

            return attribute.Value;
        }

        public static bool? GetYesNoAttribute(XPathNavigator node, string attributeName)
        {
            string attributeValue = GetAttribute(node, attributeName);

            if (attributeValue == null)
            {
                return null;
            }

            return attributeValue.Equals("yes", StringComparison.InvariantCulture);
        }

        public static PackageType? GetPackageTypeAttribute(XPathNavigator node, string attributeName)
        {
            string attributeValue = GetAttribute(node, attributeName);

            if (attributeValue == null)
            {
                return null;
            }

            if (attributeValue.Equals("Exe", StringComparison.InvariantCulture))
            {
                return PackageType.EXE;
            }
            else if (attributeValue.Equals("Msi", StringComparison.InvariantCulture))
            {
                return PackageType.MSI;
            }
            else if (attributeValue.Equals("Msp", StringComparison.InvariantCulture))
            {
                return PackageType.MSP;
            }
            else if (attributeValue.Equals("Msu", StringComparison.InvariantCulture))
            {
                return PackageType.MSU;
            }
            else
            {
                return 0;
            }
        }


    }
}
