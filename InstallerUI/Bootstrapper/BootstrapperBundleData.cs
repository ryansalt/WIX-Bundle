using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace InstallerUI.Bootstrapper
{
    public class BootstrapperBundleData
    {
        public const string defaultFileName = "BootstrapperApplicationData.xml";
        public const string xmlNamespace = "http://schemas.microsoft.com/wix/2010/BootstrapperApplicationData";

        public FileInfo DataFile { get; protected set; }
        public Bundle Data { get; protected set; }

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

        

        public BootstrapperBundleData() : this(DefaultFile) { }

        public BootstrapperBundleData(string bootstrapperBundleDataFile)
        {
            using (FileStream fs = File.OpenRead(bootstrapperBundleDataFile))
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


            bool? perMachine = GetBoolAttribute(bundleNode, "PerMachine");
            if (perMachine.HasValue)
            {
                bundle.PerMachine = perMachine.Value;
            }

            string name = GetStringAttribute(bundleNode, "DisplayName");
            if (name != null)
            {
                bundle.Name = name;
            }

            Package[] packages = ParsePackagesFromXml(root);
            bundle.Packages = packages;
            

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

                string id = GetStringAttribute(node, "Package");
                package.Id = id ?? throw new Exception("Failed to get package identifier for package");

                string displayName = GetStringAttribute(node, "DisplayName");
                if (displayName != null)
                {
                    package.DisplayName = displayName;
                }

                string description = GetStringAttribute(node, "Description");
                if (description != null)
                {
                    package.Description = description;
                }

                bool? displayInternalUI = GetBoolAttribute(node, "DisplayInternalUI");
                if (!displayInternalUI.HasValue)
                {
                    throw new Exception("Failed to get DisplayInternalUI setting for package");
                }
                package.DisplayInternalUI = displayInternalUI.Value;
                packages.Add(package);
            }

            return packages.ToArray();
        }

        public static string GetStringAttribute(XPathNavigator node, string attributeName)
        {
            XPathNavigator attribute = node.SelectSingleNode("@" + attributeName);

            if (attribute == null)
            {
                return null;
            }

            return attribute.Value;
        }

        public static bool? GetBoolAttribute(XPathNavigator node, string attributeName)
        {
            string attributeValue = GetStringAttribute(node, attributeName);

            if (attributeValue == null)
            {
                return null;
            }

            return attributeValue.Equals("yes", StringComparison.InvariantCulture);
        }
    }
}
