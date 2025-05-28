using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
   
    public partial class Video : System.Windows.Window
    {

        private VideoCapture capture;
        private CancellationTokenSource cts;


        private string videoFilePath = "";


        private string selectedFilter = "None";
        private bool isPaused = false;


        public Video()
        {
            InitializeComponent();
        }

        private void OnDownloadVideoIconClick(object sender, RoutedEventArgs e)
        {
            
        }
        private async void OnUploadVideoIconClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video files|*.mp4;*.avi;*.mov;*.mkv"
            };

            if (dialog.ShowDialog() == true)
            {

                capture = new VideoCapture(dialog.FileName);
                if (!capture.IsOpened())
                {
                    MessageBox.Show("Unable to open video.");
                    return;
                }
                videoFilePath = dialog.FileName;
                cts = new CancellationTokenSource();
                await Task.Run(() => PlayVideoAsync(cts.Token));
            }
        }


      
        private async Task PlayVideoAsync(CancellationToken token)
        {
            var frame = new Mat();
            try
            {
                while (!token.IsCancellationRequested && capture.IsOpened())
                {
                    if (isPaused)
                    {
                        await Task.Delay(100); 
                        continue;
                    }

                    if (!capture.Read(frame) || frame.Empty())
                        break;

                    Mat processedFrame = ApplySelectedFilter(frame);

                    BitmapSource bitmap = BitmapSourceConverter.ToBitmapSource(processedFrame);
                    bitmap.Freeze();

                    Dispatcher.Invoke(() =>
                    {
                        FilteredImage.Source = bitmap;
                        DrawHistograms(processedFrame);
                    });

                    await Task.Delay((int)(1000 / capture.Fps));

                    processedFrame.Dispose();
                }

                if (capture != null)
                {
                    capture.Release();
                }
            }
            finally
            {
                frame.Dispose();
            }
        }



        private void DrawHistograms(Mat frame)
        {
            if (frame.Empty())
                return;

            Mat[] bgr = Cv2.Split(frame);

            int histSize = 256;
            Rangef histRange = new Rangef(0, 256);

            
            Mat bHist = new Mat();
            Mat gHist = new Mat();
            Mat rHist = new Mat();

            Cv2.CalcHist(new Mat[] { bgr[0] }, new int[] { 0 }, null, bHist, 1, new int[] { histSize }, new Rangef[] { histRange });
            Cv2.CalcHist(new Mat[] { bgr[1] }, new int[] { 0 }, null, gHist, 1, new int[] { histSize }, new Rangef[] { histRange });
            Cv2.CalcHist(new Mat[] { bgr[2] }, new int[] { 0 }, null, rHist, 1, new int[] { histSize }, new Rangef[] { histRange });

            
            int canvasHeight = 100;
            Cv2.Normalize(bHist, bHist, 0, canvasHeight, NormTypes.MinMax);
            Cv2.Normalize(gHist, gHist, 0, canvasHeight, NormTypes.MinMax);
            Cv2.Normalize(rHist, rHist, 0, canvasHeight, NormTypes.MinMax);

           
            HistogramRedCanvas.Children.Clear();
            HistogramGreenCanvas.Children.Clear();
            HistogramBlueCanvas.Children.Clear();

            
            DrawHistogramOnCanvas(HistogramRedCanvas, rHist, Brushes.Red);
            DrawHistogramOnCanvas(HistogramGreenCanvas, gHist, Brushes.Green);
            DrawHistogramOnCanvas(HistogramBlueCanvas, bHist, Brushes.Blue);

          
            foreach (var mat in bgr)
                mat.Dispose();
            bHist.Dispose();
            gHist.Dispose();
            rHist.Dispose();
        }

        private void DrawHistogramOnCanvas(Canvas canvas, Mat hist, Brush color)
        {
            if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0)
                return;

            int histSize = 256;
            double binWidth = canvas.ActualWidth / histSize;
            double canvasHeight = canvas.ActualHeight;

            Polyline polyline = new Polyline
            {
                Stroke = color,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
                SnapsToDevicePixels = true,
            };

            for (int i = 0; i < histSize; i++)
            {
                double x = i * binWidth;
                double y = canvasHeight - hist.At<float>(i); 
                
                polyline.Points.Add(new System.Windows.Point(x, y));
            }

            canvas.Children.Add(polyline);
        }



        private void PauseVideo(object sender, RoutedEventArgs e)
        {
            isPaused = true;
        }

        private void PlayVideo(object sender, RoutedEventArgs e)
        {
            isPaused = false;
        }

        private void StopVideo(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            if (capture != null && capture.IsOpened())
            {
                capture.Release();
                capture.Dispose();
                capture = null;
            }

            FilteredImage.Source = null;
            HistogramRedCanvas.Children.Clear();
            HistogramGreenCanvas.Children.Clear();
            HistogramBlueCanvas.Children.Clear();
        }

        private async void FilteredImage_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (capture == null || !capture.IsOpened())
            {
                MessageBox.Show("No hay video cargado.");
                return;
            }

            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Video files (*.avi)|*.avi",
                FileName = "filtered_output.avi"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string outputPath = saveDialog.FileName;
                await SaveFilteredVideoAsync(outputPath);
                MessageBox.Show("Video guardado con éxito.");
            }
        }

        private async Task SaveFilteredVideoAsync(string outputPath)
        {
            using (var tempCapture = new VideoCapture(videoFilePath))
            {
                if (!tempCapture.IsOpened())
                {
                    MessageBox.Show("No se pudo abrir el video para exportar.");
                    return;
                }

                int fourcc = VideoWriter.FourCC('M', 'J', 'P', 'G'); 
                double fps = tempCapture.Fps;
                int width = tempCapture.FrameWidth;
                int height = tempCapture.FrameHeight;

                using (var writer = new VideoWriter(outputPath, fourcc, fps, new OpenCvSharp.Size(width, height)))
                {
                    var frame = new Mat();
                    while (tempCapture.Read(frame))
                    {
                        if (frame.Empty()) break;

                        Mat filtered = ApplySelectedFilter(frame);
                        writer.Write(filtered);

                        filtered.Dispose();
                        await Task.Delay(1); 
                    }

                    frame.Dispose();
                }
            }
        }


        private void OnFilterButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                selectedFilter = button.Content.ToString();
            }
        }
        private Mat ApplySelectedFilter(Mat frame)
        {
            Mat filtered = new Mat();

            switch (selectedFilter)
            {
                case "None":
                    filtered = frame.Clone();
                    break;

                case "Gaussian Blur":
                    Cv2.GaussianBlur(frame, filtered, new OpenCvSharp.Size(9, 9), 0);
                    break;

                case "Contrast Filter":
                    double alpha = 1.5; 
                    int beta = 20;     
                    Cv2.ConvertScaleAbs(frame, filtered, alpha, beta);
                    break;

                case "Mosaic Filter":
                    filtered = ApplyMosaicFilterOpenCV(frame, 10); 
                    break;

                case "Sharpness Filter":
                    filtered = ApplySharpnessFilterOpenCV(frame);
                    break;

                case "Threshold Filter":
                    int thresholdValue = 100;
                    filtered = ApplyThresholdFilterOpenCV(frame, thresholdValue);
                    break;

                case "Hue/Saturation Filter":
                    double hueShift = 45.0; // degrees
                    double saturationBoost = 1.5; // 1.0 = no change
                    filtered = ApplyHueSaturationFilterOpenCV(frame, hueShift, saturationBoost);
                    break;

                case "Negative Filter":

                    filtered = ApplyNegativeFilterOpenCV(frame);
                    break;

                case "Vignette Filter":
                    filtered = ApplyVignetteFilterOpenCV(frame);
                    break;

                case "Retro Effect":
                    filtered = ApplyRetroEffectOpenCV(frame);
                    break;

                case "Warp Filter":
                    filtered = ApplyWarpEffectOpenCV(frame, 0.3);
                    break;

                default:
                    filtered = frame.Clone();
                    break;
            }

            return filtered;
        }


        private Mat ApplyMosaicFilterOpenCV(Mat input, int blockSize)
        {
            Mat small = new Mat();
            Mat mosaic = new Mat();

            // Resize down
            Cv2.Resize(input, small, new OpenCvSharp.Size(input.Width / blockSize, input.Height / blockSize), interpolation: InterpolationFlags.Nearest);

            // Resize up
            Cv2.Resize(small, mosaic, new OpenCvSharp.Size(input.Width, input.Height), interpolation: InterpolationFlags.Nearest);

            small.Dispose();
            return mosaic;
        }
        private Mat ApplySharpnessFilterOpenCV(Mat input)
        {
            Mat output = new Mat();

            Mat kernel = new Mat(3, 3, MatType.CV_32F);

            // Set the kernel values manually (stronger sharpness)
            kernel.Set(0, 0, -1f);
            kernel.Set(0, 1, -1f);
            kernel.Set(0, 2, -1f);
            kernel.Set(1, 0, -1f);
            kernel.Set(1, 1, 9f);
            kernel.Set(1, 2, -1f);
            kernel.Set(2, 0, -1f);
            kernel.Set(2, 1, -1f);
            kernel.Set(2, 2, -1f);

            // Apply the filter
            Cv2.Filter2D(input, output, input.Type(), kernel);

            return output;
        }
        private Mat ApplyThresholdFilterOpenCV(Mat input, int threshold)
        {
            Mat gray = new Mat();
            Mat binary = new Mat();
            Mat binaryBgr = new Mat();

            Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.Threshold(gray, binary, threshold, 255, ThresholdTypes.Binary);

            Cv2.CvtColor(binary, binaryBgr, ColorConversionCodes.GRAY2BGR);

            gray.Dispose();
            binary.Dispose();

            return binaryBgr;
        }

        private Mat ApplyHueSaturationFilterOpenCV(Mat input, double hueShiftDegrees, double saturationFactor)
        {
            Mat hsv = new Mat();
            Mat result = new Mat();

            // Convert input from BGR to HSV
            Cv2.CvtColor(input, hsv, ColorConversionCodes.BGR2HSV);

            // Split channels
            Mat[] hsvChannels = Cv2.Split(hsv);

            Mat hue = hsvChannels[0];
            Mat saturation = hsvChannels[1];
            Mat value = hsvChannels[2];

            // ---- Apply hue shift ----
            double hueShift = hueShiftDegrees / 2.0; // OpenCV uses 0-179 for Hue

            for (int y = 0; y < hue.Rows; y++)
            {
                for (int x = 0; x < hue.Cols; x++)
                {
                    byte oldHue = hue.At<byte>(y, x);
                    int newHue = (int)(oldHue + hueShift);
                    if (newHue < 0) newHue += 180;
                    if (newHue >= 180) newHue -= 180;
                    hue.Set<byte>(y, x, (byte)newHue);
                }
            }

            // ---- Apply saturation scaling ----
            for (int y = 0; y < saturation.Rows; y++)
            {
                for (int x = 0; x < saturation.Cols; x++)
                {
                    byte oldSaturation = saturation.At<byte>(y, x);
                    int newSaturation = Clamp((int)(oldSaturation * saturationFactor), 0, 255);
                    saturation.Set<byte>(y, x, (byte)newSaturation);
                }
            }

            // Merge channels back
            Cv2.Merge(new Mat[] { hue, saturation, value }, hsv);

            // Convert back to BGR
            Cv2.CvtColor(hsv, result, ColorConversionCodes.HSV2BGR);

            return result;
        }
        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private Mat ApplyNegativeFilterOpenCV(Mat input)
        {
            Mat output = new Mat();

            // Invert the image colors
            Cv2.BitwiseNot(input, output);

            return output;
        }
        public Mat ApplyVignetteFilterOpenCV(Mat frame)
        {
            int width = frame.Width;
            int height = frame.Height;

            // Get the center of the image
            double centerX = width / 2.0;
            double centerY = height / 2.0;

            // Calculate maximum distance from center (diagonal distance)
            double maxDistance = Math.Sqrt(centerX * centerX + centerY * centerY);

            // Clone the original frame to modify
            Mat vignetteFrame = frame.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get the pixel at (x, y)
                    Vec3b pixel = frame.At<Vec3b>(y, x);

                    // Calculate the distance from the center
                    double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));

                    // Calculate vignette strength (value between 0 and 1)
                    double vignetteStrength = 1 - (distance / maxDistance);

                    // Apply vignette effect (reduce brightness based on distance)
                    // Multiply each color channel (Blue, Green, Red) by vignette strength
                    for (int channel = 0; channel < 3; channel++) // BGR channels
                    {
                        byte newValue = (byte)(pixel[channel] * vignetteStrength);
                        vignetteFrame.Set(y, x, new Vec3b(
                            channel == 0 ? newValue : pixel[0], // Blue
                            channel == 1 ? newValue : pixel[1], // Green
                            channel == 2 ? newValue : pixel[2]  // Red
                        ));
                    }
                }
            }

            return vignetteFrame; // Return the modified image
        }
        public Mat ApplyRetroEffectOpenCV(Mat frame)
        {
            int width = frame.Width;
            int height = frame.Height;

            // Clone the original frame to modify it
            Mat retroFrame = frame.Clone();

            // Apply desaturation (faded effect)
            double saturationFactor = 0.6; // A value between 0 (grayscale) and 1 (full color)

            // Apply a warm brownish tint (similar to old photos)
            byte tintRed = 70, tintGreen = 50, tintBlue = 30; // Brownish tint (Blue, Green, Red)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get the pixel at (x, y)
                    Vec3b pixel = frame.At<Vec3b>(y, x);

                    // Extract RGB values (OpenCV stores them as BGR)
                    byte blue = pixel[0];
                    byte green = pixel[1];
                    byte red = pixel[2];

                    // Desaturate the color by calculating the average
                    byte gray = (byte)((red + green + blue) / 3);

                    // Blend with original color using saturation factor
                    red = (byte)(gray + (red - gray) * saturationFactor);
                    green = (byte)(gray + (green - gray) * saturationFactor);
                    blue = (byte)(gray + (blue - gray) * saturationFactor);

                    // Apply the brownish tint
                    red = (byte)Math.Min(255, red + tintRed);
                    green = (byte)Math.Min(255, green + tintGreen);
                    blue = (byte)Math.Min(255, blue + tintBlue);

                    // Set the modified pixel back to the frame
                    retroFrame.Set(y, x, new Vec3b(blue, green, red)); // BGR order
                }
            }

            return retroFrame; // Return the retro-effected image
        }
        public static Mat ApplyWarpEffectOpenCV(Mat frame, double warpStrength)
        {
            int width = frame.Width;
            int height = frame.Height;

            // Create an empty Mat for the warped image
            Mat warpedFrame = new Mat(height, width, frame.Type());

            // Get the center of the image
            int centerX = width / 2;
            int centerY = height / 2;

            // Create maps for remapping
            Mat mapX = new Mat(height, width, MatType.CV_32F);
            Mat mapY = new Mat(height, width, MatType.CV_32F);

            // Loop through each pixel and apply the warp effect
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double factor = 1 + warpStrength * Math.Sin(distance / 30); // Warp effect based on sine

                    // Calculate the new positions based on the warp factor
                    float newX = (float)(centerX + dx * factor);
                    float newY = (float)(centerY + dy * factor);

                    // Set the calculated values into the map matrices
                    mapX.At<float>(y, x) = newX;
                    mapY.At<float>(y, x) = newY;
                }
            }

            // Apply the remap function to warp the image
            Cv2.Remap(frame, warpedFrame, mapX, mapY, InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(0));

            return warpedFrame; // Return the warped image
        }

        private void GoToVideoPage(object sender, RoutedEventArgs e)
        {

        }
        private void GoToCameraPage(object sender, RoutedEventArgs e)
        {

            Camera newWindow = new Camera();
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.Show();


            this.Close();
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
            MessageBox.Show("Software version: 1.000", "Picture Maker Version:", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void UserManualClick(object sender, RoutedEventArgs e)
        {
            string url = "https://firebasestorage.googleapis.com/v0/b/studify-707e5/o/MID-picture-lab-manual-de-usuario.pdf?alt=media&token=b06e1bde-cadc-446f-a533-7dd3db592cd2";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

    }
}
