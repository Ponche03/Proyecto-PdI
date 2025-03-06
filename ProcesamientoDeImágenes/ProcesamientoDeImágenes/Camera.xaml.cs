using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ProcesamientoDeImágenes
{
    /// <summary>
    /// Interaction logic for Camera.xaml
    /// </summary>
    public partial class Camera : Window
    {
        public Camera()
        {
            InitializeComponent();
        }

        private void GoToVideoPage(object sender, RoutedEventArgs e)
        {
            Video newWindow = new Video();
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.Show();
           

            this.Close();
        }

        private void GoToCameraPage(object sender, RoutedEventArgs e)
        {
          
        }

        private void GoToImageWindow(object sender, RoutedEventArgs e)
        {

            MainWindow newWindow = new MainWindow();
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.Show();
           

            this.Close();

        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SoftwareInfoClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Software version: 1.000", "Software Version", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UserManualClick(object sender, RoutedEventArgs e)
        {
            string url = "https://drive.google.com/file/d/1SZQoL3c3r61YYPyqq00cw3CIdI_oyrap/view?usp=drive_link";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void OnFlipImageIconClick(object sender, RoutedEventArgs e)
        {
            // Handle image flip functionality here
        }

        private void OnScreenShotIconClick(object sender, RoutedEventArgs e)
        {
            // Handle screenshot functionality here
        }

        private void OnFaceDetectIconClick(object sender, RoutedEventArgs e)
        {
            // Handle face detection functionality here
        }



    }
}
