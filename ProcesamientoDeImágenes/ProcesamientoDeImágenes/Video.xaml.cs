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
using System.Windows.Shapes;

namespace ProcesamientoDeImágenes
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : Window
    {
        public Video()
        {
            InitializeComponent();
        }

        private void GoToVideoPage(object sender, RoutedEventArgs e)
        {
           
        }

        private void GoToCameraPage(object sender, RoutedEventArgs e)
        {

            Camera newWindow = new Camera();
            newWindow.Show();

            this.Close();
        }

        private void GoToImageWindow(object sender, RoutedEventArgs e)
        {


            MainWindow newWindow = new MainWindow();
            newWindow.Show();

            this.Close();

        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
