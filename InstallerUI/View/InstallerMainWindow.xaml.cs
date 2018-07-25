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

        private void Hyperlink_OpenAgreement(object sender, RoutedEventArgs e)
        {
            try
            {
                //string fileName = Path.GetTempPath() + "License.pdf";
                //Process process = new Process();
                //process.StartInfo.FileName = fileName;
                //process.Start();
            }
            catch
            {

            }
        }

        private void LicenseAgreement_Checked(object sender, RoutedEventArgs e)
        {
            BtnInstall.IsEnabled = true;
            // LicenseAgreementCheckBox.IsEnabled = false;
        }

        private void LicenseAgreement_UnChecked(object sender, RoutedEventArgs e)
        {
            BtnInstall.IsEnabled = false;
        }

        private void BtnAutentication_Click(object sender, RoutedEventArgs e)
        {
            //ValidateUserAndTokenAsync();
        }

        private void EnterClicked(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //ValidateUserAndTokenAsync();
                e.Handled = true;
            }
        }

        //private async void ValidateUserAndTokenAsync()
        //{
        //    string userName = User.Text;
        //    string userPass = Password.Password.ToString();
        //    var databaseConfig = await AuthService.AuthenticateUser(userName, userPass);
        //    if (databaseConfig != null)
        //    {
        //        AuthenticationGrid.Visibility = Visibility.Collapsed;
        //        InstallerGrid.Visibility = Visibility.Visible;
        //        //_viewModel.DatabaseConfig = databaseConfig;
        //    }
        //    else
        //    {
        //        LabelAuthError.Visibility = Visibility.Visible;
        //        User.Text = string.Empty;
        //        Password.Clear();
        //    }
        //}
    }
}
