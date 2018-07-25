using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using InstallerUI.Data;
using InstallerUI.Interfaces;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Newtonsoft.Json;

namespace InstallerUI.Bootstrapper
{
    public class InstallerUIBootsrapper : BootstrapperApplication, IInteractionService
    {
        private BootstrapperApplicationData bootstrapperApplicationData;
        private Window installerMainWindow;
        private IntPtr installerMainWindowHandle;

        protected override void Run()
        {
            Engine.Log(LogLevel.Verbose, "Entry point of WiX - Run method");
            using (var container = SetupCompositionContainer())
            {
                bootstrapperApplicationData = new BootstrapperApplicationData();
                Engine.Log(LogLevel.Verbose, JsonConvert.SerializeObject(bootstrapperApplicationData));

                // Create main window with associated view model
                Engine.Log(LogLevel.Verbose, "Creating a UI.");
                installerMainWindow = container.GetExportedValue<Window>("InstallerMainWindow");
                Engine.Log(LogLevel.Verbose, "Creating a UI 1.");
                installerMainWindowHandle = new WindowInteropHelper(installerMainWindow).EnsureHandle();
                Engine.Log(LogLevel.Verbose, "Creating a UI 2.");

                Engine.Detect();
                Engine.Log(LogLevel.Verbose, "Creating a UI 3.");
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

        public void ShowMessageBox(string message)
        {
            installerMainWindow.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(message)), null);
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
