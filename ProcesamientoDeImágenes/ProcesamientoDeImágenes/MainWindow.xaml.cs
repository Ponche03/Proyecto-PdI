using System;
using System.Collections.Generic;
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

        private void GoToVideoPage(object sender, RoutedEventArgs e)
        {
            Video newWindow = new Video();
            newWindow.Show();

            this.Close(); 
        }

        private void GoToCameraPage(object sender, RoutedEventArgs e)
        {
            Camera newWindow = new Camera();
            newWindow.Show();

            this.Close();
        }

        private void GoToImageWindow(object sender, RoutedEventArgs e)
        {
            
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

     

    }

}
