using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ProcesamientoDeImágenes
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoToImageWindow(object sender, RoutedEventArgs e)
        {

        }
        private void GoToVideoPage(object sender, RoutedEventArgs e)
        {
            Video newWindow = new Video();
            newWindow.Show();
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.Close(); 
        }

        private void GoToCameraPage(object sender, RoutedEventArgs e)
        {
            Camera newWindow = new Camera();
            newWindow.Show();
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

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



        private void OnZoomIconClick(object sender, RoutedEventArgs e)
        {
            // Handle zoom functionality here
        }

        private void OnFlipIconClick(object sender, RoutedEventArgs e)
        {
            // Handle flip image functionality here
        }

        private void OnUploadImageIconClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select an Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
          
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                UploadedImage.Source = bitmap;  
            }
        }

        private void OnDownloadImageIconClick(object sender, RoutedEventArgs e)
        {
            // Handle image download functionality here
        }


    }

}
