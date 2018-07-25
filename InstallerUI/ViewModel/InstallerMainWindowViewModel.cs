using InstallerUI.Bootstrapper;
using InstallerUI.Data;
using InstallerUI.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InstallerUI.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class InstallerMainWindowViewModel : BindableBase
    {
        private BootstrapperApplication bootstrapper;
        private Engine engine;
        private readonly BootstrapperApplicationData bootstrapperApplicationData;

        [Import]
        private IInteractionService interactionService = null;

        #region Properties for data binding
        private DelegateCommand InstallCommandValue;
        public ICommand InstallCommand { get { return InstallCommandValue; } }

        private DelegateCommand UninstallCommandValue;
        public ICommand UninstallCommand { get { return UninstallCommandValue; } }

        private DelegateCommand CancelCommandValue;
        public ICommand CancelCommand { get { return CancelCommandValue; } }

        private InstallationState StateValue;
        public InstallationState State
        {
            get => StateValue;
            set
            {
                SetProperty(ref StateValue, value);
                InstallCommandValue.RaiseCanExecuteChanged();
                UninstallCommandValue.RaiseCanExecuteChanged();
            }
        }

        private bool DowngradeValue;
        public bool Downgrade
        {
            get => DowngradeValue;
            set => SetProperty(ref DowngradeValue, value);
        }

        private int LocalProgressValue;
        public int LocalProgress
        {
            get => LocalProgressValue;
            set => SetProperty(ref LocalProgressValue, value);
        }

        private int GlobalProgressValue;
        public int GlobalProgress
        {
            get => GlobalProgressValue;
            set => SetProperty(ref GlobalProgressValue, value);
        }

        private string ProgressValue;
        public string Progress
        {
            get => ProgressValue;
            set => SetProperty(ref ProgressValue, value);
        }

        private string CurrentPackageValue;
        public string CurrentPackage
        {
            get => CurrentPackageValue;
            set => SetProperty(ref CurrentPackageValue, value);
        }

        private bool InstallingValue;
        public bool Installing
        {
            get => InstallingValue;
            set
            {
                SetProperty(ref InstallingValue, value);
                InstallCommandValue.RaiseCanExecuteChanged();
                UninstallCommandValue.RaiseCanExecuteChanged();
            }
        }

        private bool isCancelledValue;
        public bool IsCancelled
        {
            get => isCancelledValue;
            set
            {
                SetProperty(ref isCancelledValue, value);
            }
        }

        private Visibility AuthenticationGridVisibility;
        public Visibility AuthenticationGridVisible
        {
            get => AuthenticationGridVisibility;
            set => SetProperty(ref this.AuthenticationGridVisibility, value);

        }

        private Visibility InstallerGridVisibility;
        public Visibility InstallerGridVisible
        {
            get => InstallerGridVisibility;
            set => SetProperty(ref this.InstallerGridVisibility, value);
        }

        private Visibility LicenseVisibility;
        public Visibility LicenseVisible
        {
            get => LicenseVisibility;
            set => SetProperty(ref this.LicenseVisibility, value);
        }

        
        #endregion

        [ImportingConstructor]
        public InstallerMainWindowViewModel(BootstrapperApplication bootstrapper, Engine engine)
        {
            bootstrapperApplicationData = new BootstrapperApplicationData();
            this.bootstrapper = bootstrapper;
            this.engine = engine;

            // Setup commands
            InstallCommandValue = new DelegateCommand(
                () => engine.Plan(LaunchAction.Install),
                () => !Installing && State == InstallationState.DetectedAbsent);
            UninstallCommandValue = new DelegateCommand(
                () => engine.Plan(LaunchAction.Uninstall),
                () => !Installing && State == InstallationState.DetectedPresent);
            CancelCommandValue = new DelegateCommand(
                () => IsCancelled = true);


            // Setup event handlers
            bootstrapper.DetectBegin += (_, ea) =>
            {
                LogEvent("DetectBegin", ea);

                //Write the temporary license file after it was read from embedded file
                //Service.CreateTempLicense();
                // Set installation state that controls the install/uninstall buttons
                interactionService.RunOnUIThread(
                    () => State = ea.Installed ? InstallationState.DetectedPresent : InstallationState.DetectedAbsent);
            };

            bootstrapper.DetectRelatedBundle += (_, ea) =>
            {
                LogEvent("DetectRelatedBundle", ea);

                // Save flag indicating whether this is a downgrade operation
                interactionService.RunOnUIThread(() => Downgrade |= ea.Operation == RelatedOperation.Downgrade);
            };

            bootstrapper.DetectComplete += (s, ea) =>
            {
                LogEvent("DetectComplete");
                DetectComplete(s, ea);
            };

            bootstrapper.PlanComplete += (_, ea) =>
            {
                LogEvent("PlanComplete", ea);

                // Start apply phase
                if (ea.Status >= 0 /* Success */)
                {
                    engine.Apply(interactionService.GetMainWindowHandle());
                }
            };

            bootstrapper.ApplyBegin += (_, ea) =>
            {
                LogEvent("ApplyBegin");

                // Set flag indicating that apply phase is running
                interactionService.RunOnUIThread(() => Installing = true);
            };

            bootstrapper.ExecutePackageBegin += (_, ea) =>
            {
                LogEvent("ExecutePackageBegin", ea);

                // Trigger display of currently processed package
                interactionService.RunOnUIThread(() =>
                CurrentPackage = String.Format("Current package: {0}",
                bootstrapperApplicationData.Data.Packages.Where(p => p.Id == ea.PackageId).FirstOrDefault().DisplayName));
            };

            bootstrapper.ExecutePackageComplete += (_, ea) =>
            {
                if (State == InstallationState.DetectedAbsent && IsCancelled != true)
                {

                    if (ea.PackageId == "MySQL")
                    {
                        LogEvent("StatSports-Installer>> Current Action: " + this.bootstrapper.Command.Action);
                        LogEvent("StatSports-Installer>> Before MySQL server init and schema creation");

                        //Service.InitServer(port, DatabaseConfig);
                        //Service.CreateSchema(DatabaseConfig);
                    }

                    // If whole install is finished then recheck port 20499
                    if (ea.PackageId == "Electron")
                    {
                        //if (Service.CheckPort(port))
                        //{
                        //    //Service.CreateInstance(port, DatabaseConfig);
                        //    //Service.CreateSchema(DatabaseConfig);
                        //}
                    }
                }

                LogEvent("ExecutePackageComplete", ea);
                // Remove currently processed package
                interactionService.RunOnUIThread(() => CurrentPackage = string.Empty);
            };

            bootstrapper.ExecuteProgress += (_, ea) =>
            {
                LogEvent("ExecuteProgress", ea);
                if (IsCancelled == true)
                {
                    ea.Result = Result.Abort;
                }
                Progress = String.Format("Progress: {0}{1}", ea.OverallPercentage.ToString(), "%");

                // Update progress indicator
                interactionService.RunOnUIThread(() =>
                {
                    LocalProgress = ea.ProgressPercentage;
                    GlobalProgress = ea.OverallPercentage;
                });
            };

            bootstrapper.ApplyComplete += (_, ea) =>
            {
                LogEvent("ApplyComplete", ea);

                // Everything is done, let's close the installer
                interactionService.CloseUIAndExit();
            };
        }

        private void SetupEventHandlersForLogging()
        {
            bootstrapper.Startup += (_, ea) => LogEvent("Startup");
            bootstrapper.Shutdown += (_, ea) => LogEvent("Shutdown");
            bootstrapper.SystemShutdown += (_, ea) => LogEvent("SystemShutdown", ea);
            bootstrapper.DetectCompatiblePackage += (_, ea) => LogEvent("DetectCompatiblePackage", ea);
            bootstrapper.DetectForwardCompatibleBundle += (_, ea) => LogEvent("DetectForwardCompatibleBundle", ea);
            bootstrapper.DetectMsiFeature += (_, ea) => LogEvent("DetectMsiFeature", ea);
            bootstrapper.DetectPackageBegin += (_, ea) => LogEvent("DetectPackageBegin", ea);
            bootstrapper.DetectPackageComplete += (_, ea) => LogEvent("DetectPackageComplete", ea);
            bootstrapper.DetectPriorBundle += (_, ea) => LogEvent("DetectPriorBundle", ea);
            bootstrapper.DetectRelatedMsiPackage += (_, ea) => LogEvent("DetectRelatedMsiPackage", ea);
            bootstrapper.DetectTargetMsiPackage += (_, ea) => LogEvent("DetectTargetMsiPackage", ea);
            bootstrapper.DetectUpdate += (_, ea) => LogEvent("DetectUpdate", ea);
            bootstrapper.DetectUpdateBegin += (_, ea) => LogEvent("DetectUpdateBegin", ea);
            bootstrapper.DetectUpdateComplete += (_, ea) => LogEvent("DetectUpdateComplete", ea);
            bootstrapper.Elevate += (_, ea) => LogEvent("Elevate", ea);
            bootstrapper.Error += (_, ea) => LogEvent("Error", ea);
            bootstrapper.ExecuteBegin += (_, ea) => LogEvent("ExecuteBegin", ea);
            bootstrapper.ExecuteComplete += (_, ea) => LogEvent("ExecuteComplete", ea);
            bootstrapper.ExecuteFilesInUse += (_, ea) => LogEvent("ExecuteFilesInUse", ea);
            bootstrapper.ExecuteMsiMessage += (_, ea) => LogEvent("ExecuteMsiMessage", ea);
            bootstrapper.ExecutePatchTarget += (_, ea) => LogEvent("ExecutePatchTarget", ea);
            bootstrapper.LaunchApprovedExeBegin += (_, ea) => LogEvent("LaunchApprovedExeBegin");
            bootstrapper.LaunchApprovedExeComplete += (_, ea) => LogEvent("LaunchApprovedExeComplete", ea);
            bootstrapper.PlanBegin += (_, ea) => LogEvent("PlanBegin", ea);
            bootstrapper.PlanCompatiblePackage += (_, ea) => LogEvent("PlanCompatiblePackage", ea);
            bootstrapper.PlanMsiFeature += (_, ea) => LogEvent("PlanMsiFeature", ea);
            bootstrapper.PlanPackageBegin += (_, ea) => LogEvent("PlanPackageBegin", ea);
            bootstrapper.PlanPackageComplete += (_, ea) => LogEvent("PlanPackageComplete", ea);
            bootstrapper.PlanRelatedBundle += (_, ea) => LogEvent("PlanRelatedBundle", ea);
            bootstrapper.PlanTargetMsiPackage += (_, ea) => LogEvent("PlanTargetMsiPackage", ea);
            bootstrapper.Progress += (_, ea) => LogEvent("Progress", ea);
            bootstrapper.RegisterBegin += (_, ea) => LogEvent("RegisterBegin");
            bootstrapper.RegisterComplete += (_, ea) => LogEvent("RegisterComplete", ea);
            bootstrapper.ResolveSource += (_, ea) => LogEvent("ResolveSource", ea);
            bootstrapper.RestartRequired += (_, ea) => LogEvent("RestartRequired", ea);
            bootstrapper.UnregisterBegin += (_, ea) => LogEvent("UnregisterBegin", ea);
            bootstrapper.UnregisterComplete += (_, ea) => LogEvent("UnregisterComplete", ea);
        }



        private void DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            // If necessary, parse the command line string before any planning
            // (e.g. detect installation folder)

            if (LaunchAction.Uninstall == bootstrapper.Command.Action)
            {
                engine.Log(LogLevel.Verbose, "Invoking automatic plan for uninstall");
                engine.Plan(LaunchAction.Uninstall);
            }
            else if (e.Status >= 0 /* Success */)
            {
                if (Downgrade)
                {
                    // What do you want to do in case of downgrade?
                    // Here: Stop installation

                    string message = "Sorry, we do not support downgrades.";
                    engine.Log(LogLevel.Verbose, message);
                    if (bootstrapper.Command.Display == Display.Full)
                    {
                        interactionService.ShowMessageBox(message);
                        interactionService.CloseUIAndExit();
                    }
                }

                if (bootstrapper.Command.Action == LaunchAction.Layout)
                {
                    // Copies all of the Bundle content to a specified directory
                    engine.Plan(LaunchAction.Layout);
                }
                else if (bootstrapper.Command.Display != Display.Full)
                {
                    // If we're not waiting for the user to click install, dispatch plan with the default action.
                    engine.Log(LogLevel.Verbose, "Invoking automatic plan for non-interactive mode.");
                    engine.Plan(LaunchAction.Install);
                }
            }
        }

        private void LogEvent(string eventName, EventArgs arguments = null)
        {
            engine.Log(
                LogLevel.Verbose,
                arguments == null ? string.Format("EVENT: {0}", eventName)
                                    :
                                    string.Format("EVENT: {0} ({1})",
                                                  eventName,
                                                  JsonConvert.SerializeObject(arguments))
            );
        }
    }
}
