using InstallerUI.ViewModel;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System.ComponentModel.Composition;
using System.Windows;

namespace InstallerUI.View
{
    /// <summary>
    /// Interaction logic for InstallerUIWindow.xaml
    /// </summary>
    [Export("InstallerUIWindow", typeof(Window))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class InstallerUIWindow : Window
    {


        [ImportingConstructor]
        public InstallerUIWindow(InstallerUIWindowViewModel viewModel, Engine engine)
        {
            DataContext = viewModel;

            Loaded += (sender, e) => engine.CloseSplashScreen();
            Closed += (sender, e) => Dispatcher.InvokeShutdown(); // shutdown dispatcher when the window is closed.
            MouseLeftButtonDown += (sender, e) => DragMove();

            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


    }
}
