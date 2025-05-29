using System;
using System.Windows;
using System.Windows.Controls;
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

            int a_lab = a - 128;
            int b_lab = b - 128;

            CoordinatesTextBlock.Text = $"Coordenadas: ({x}, {y})";
            LTextBlock.Text = $"L: {l}";
            ATextBlock.Text = $"a: {a_lab}";
            BTextBlock.Text = $"b: {b_lab}";

            DrawLabCircle(a, b);
        }

        private void DrawLabCircle(int a, int b)
        {
            double canvasSize = LabCanvas.Width; // Asume canvas cuadrado
            double center = canvasSize / 2.0;

            LabCanvas.Children.Clear();

            LabCanvas.Background = GenerateLabBackground();

            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = center,
                X2 = canvasSize,
                Y2 = center,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            Line yAxis = new Line
            {
                X1 = center,
                Y1 = 0,
                X2 = center,
                Y2 = canvasSize,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            LabCanvas.Children.Add(xAxis);
            LabCanvas.Children.Add(yAxis);

            // --- Aquí agrega las etiquetas de los ejes ---
            int minLab = -128;
            int maxLab = 127;
            int centerLab = 0;

            var fontSize = 12.0;
            var brush = Brushes.Black;

            // Texto eje X
            TextBlock leftLabel = new TextBlock { Text = minLab.ToString(), FontSize = fontSize, Foreground = brush };
            TextBlock centerXLabel = new TextBlock { Text = centerLab.ToString(), FontSize = fontSize, Foreground = brush };
            TextBlock rightLabel = new TextBlock { Text = maxLab.ToString(), FontSize = fontSize, Foreground = brush };

            // Texto eje Y
            TextBlock topLabel = new TextBlock { Text = maxLab.ToString(), FontSize = fontSize, Foreground = brush };
            TextBlock centerYLabel = new TextBlock { Text = centerLab.ToString(), FontSize = fontSize, Foreground = brush };
            TextBlock bottomLabel = new TextBlock { Text = minLab.ToString(), FontSize = fontSize, Foreground = brush };

            Canvas.SetLeft(leftLabel, 0);
            Canvas.SetTop(leftLabel, center + 2);

            Canvas.SetLeft(centerXLabel, center - 10);
            Canvas.SetTop(centerXLabel, center + 2);

            Canvas.SetLeft(rightLabel, canvasSize - 20);
            Canvas.SetTop(rightLabel, center + 2);

            Canvas.SetLeft(topLabel, center + 4);
            Canvas.SetTop(topLabel, 0);

            Canvas.SetLeft(centerYLabel, center + 4);
            Canvas.SetTop(centerYLabel, center - 10);

            Canvas.SetLeft(bottomLabel, center + 4);
            Canvas.SetTop(bottomLabel, canvasSize - 20);

            LabCanvas.Children.Add(leftLabel);
            LabCanvas.Children.Add(centerXLabel);
            LabCanvas.Children.Add(rightLabel);
            LabCanvas.Children.Add(topLabel);
            LabCanvas.Children.Add(centerYLabel);
            LabCanvas.Children.Add(bottomLabel);
            // --- Fin etiquetas ---

            Ellipse circle = new Ellipse
            {
                Width = canvasSize,
                Height = canvasSize,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(circle, 0);
            Canvas.SetTop(circle, 0);
            LabCanvas.Children.Add(circle);

            // Ajuste para convertir byte [0..255] a rango LAB [-128..127]
            int a_lab = a - 128;
            int b_lab = b - 128;

            double scale = canvasSize / 255.0;

            double canvasX = center + (a_lab * scale);
            double canvasY = center - (b_lab * scale); // Invertir Y

            canvasX = Math.Max(3, Math.Min(canvasSize - 3, canvasX));
            canvasY = Math.Max(3, Math.Min(canvasSize - 3, canvasY));

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(point, canvasX - 3);
            Canvas.SetTop(point, canvasY - 3);
            LabCanvas.Children.Add(point);
        }


        private ImageBrush GenerateLabBackground()
        {
            const int canvasSize = 200;
            const int scale = 2;
            const int center = canvasSize / 2;
            const double fixedL = 50.0;

            WriteableBitmap bitmap = new WriteableBitmap(canvasSize, canvasSize, 96, 96, PixelFormats.Bgr32, null);
            int[] pixels = new int[canvasSize * canvasSize];

            for (int y = 0; y < canvasSize; y++)
            {
                for (int x = 0; x < canvasSize; x++)
                {
                    int a = (x - center) / scale;
                    int b = (center - y) / scale;

                    Color color = LabToRgb(fixedL, a, b);
                    int pixelColor = (color.R << 16) | (color.G << 8) | color.B;
                    pixels[y * canvasSize + x] = pixelColor;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, canvasSize, canvasSize), pixels, canvasSize * 4, 0);

            return new ImageBrush(bitmap);
        }


        private Color LabToRgb(double L, double a, double b)
        {
            // Convertir de Lab a XYZ
            double y = (L + 16.0) / 116.0;
            double x = a / 500.0 + y;
            double z = y - b / 200.0;

            double x3 = Math.Pow(x, 3);
            double z3 = Math.Pow(z, 3);
            x = 0.95047 * ((x3 > 0.008856) ? x3 : (x - 16.0 / 116.0) / 7.787);
            y = 1.00000 * ((L > 7.9996) ? Math.Pow(y, 3) : L / 903.3);
            z = 1.08883 * ((z3 > 0.008856) ? z3 : (z - 16.0 / 116.0) / 7.787);

            // Convertir de XYZ a sRGB
            double r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            double g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            double bVal = x * 0.0557 + y * -0.2040 + z * 1.0570;

            // Compandado gamma
            Func<double, double> GammaCorrect = c =>
            {
                c = (c > 0.0031308) ? 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055 : 12.92 * c;
                return Math.Max(0, Math.Min(1, c));
            };

            r = GammaCorrect(r);
            g = GammaCorrect(g);
            bVal = GammaCorrect(bVal);

            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(bVal * 255));
        }



    }

}