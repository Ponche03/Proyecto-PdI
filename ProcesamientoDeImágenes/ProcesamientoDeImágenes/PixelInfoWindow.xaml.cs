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

    public partial class PixelInfoWindow : Window
    {
        public PixelInfoWindow(int x, int y, byte l, byte a, byte b)
        {
            InitializeComponent();
            CoordinatesTextBlock.Text = $"Pixel Coordinates: ({x}, {y})";
            LTextBlock.Text = $"L (Lightness): {l}"; // OpenCV's L is 0-255
            ATextBlock.Text = $"A (Green-Red): {a}"; // OpenCV's A is 0-255
            BTextBlock.Text = $"B (Blue-Yellow): {b}"; // OpenCV's B is 0-255
        }
    }
}
