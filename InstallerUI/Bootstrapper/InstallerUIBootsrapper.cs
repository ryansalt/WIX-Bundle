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
    public class InstallerUIBootsrapper : BootstrapperApplication, IInteractionService
    {
        private BootstrapperBundleData bootstrapperBundleData;
        private Window installerMainWindow;
        private IntPtr installerMainWindowHandle;

        protected override void Run()
        {
            Engine.Log(LogLevel.Verbose, "Entry point of WiX - Run method");
            using (var container = SetupCompositionContainer())
            {
                bootstrapperBundleData = new BootstrapperBundleData();
                Engine.Log(LogLevel.Verbose, JsonConvert.SerializeObject(bootstrapperBundleData));

                // Create main window with associated view model
                installerMainWindow = container.GetExportedValue<Window>("InstallerMainWindow");
                installerMainWindowHandle = new WindowInteropHelper(installerMainWindow).EnsureHandle();

                Engine.Detect();
                if (Command.Display == Display.Passive || Command.Display == Display.Full)
                {
                    installerMainWindow.Show();
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
            installerMainWindow.Dispatcher.BeginInvoke(new Action(() => installerMainWindow.Close()));
        }

        public void RunOnUIThread(Action body)
        {
            installerMainWindow.Dispatcher.BeginInvoke(body, null);
        }

        public IntPtr GetMainWindowHandle()
        {
            return installerMainWindowHandle;
        }
    }
}
