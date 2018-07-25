using InstallerUI.ViewModel;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstallerUI.View
{
    /// <summary>
    /// Interaction logic for InstallerMainWindow.xaml
    /// </summary>
    [Export("InstallerMainWindow", typeof(Window))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class InstallerMainWindow : Window
    {


        [ImportingConstructor]
        public InstallerMainWindow(InstallerMainWindowViewModel viewModel, Engine engine)
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
