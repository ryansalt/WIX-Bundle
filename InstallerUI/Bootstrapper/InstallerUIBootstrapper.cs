using InstallerUI.Interfaces;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace InstallerUI.Bootstrapper
{
    public class InstallerUIBootstrapper : BootstrapperApplication, IInteractionService
    {
        private BootstrapperBundleData bootstrapperBundleData;
        private Window installerUIWindow;
        private IntPtr installerUIWindowHandle;

        protected override void Run()
        {
            Engine.Log(LogLevel.Verbose, "Entry point of WiX - Run method");
            using (var container = SetupCompositionContainer())
            {
                bootstrapperBundleData = new BootstrapperBundleData();
                Engine.Log(LogLevel.Verbose, JsonConvert.SerializeObject(bootstrapperBundleData));

                // Create main window with associated view model
                installerUIWindow = container.GetExportedValue<Window>("InstallerUIWindow");
                installerUIWindowHandle = new WindowInteropHelper(installerUIWindow).EnsureHandle();

                Engine.Detect();
                if (Command.Display == Display.Passive || Command.Display == Display.Full)
                {
                    installerUIWindow.Show();
                }
                Dispatcher.Run();

                Engine.Quit(0);
                Engine.Log(LogLevel.Verbose, "Exiting custom WPF UI.");
            }

        }

        private CompositionContainer SetupCompositionContainer()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeExportedValue<BootstrapperApplication>(this);
            container.ComposeExportedValue<Engine>(Engine);
            container.ComposeExportedValue<IInteractionService>(this);
            return container;
        }

        public void CloseUIAndExit()
        {
            installerUIWindow.Dispatcher.BeginInvoke(new Action(() => installerUIWindow.Close()));
        }

        public void RunOnUIThread(Action body)
        {
            installerUIWindow.Dispatcher.BeginInvoke(body, null);
        }

        public IntPtr GetMainWindowHandle()
        {
            return installerUIWindowHandle;
        }
    }
}
