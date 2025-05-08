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
using System.Diagnostics;

using System.Drawing;
using System.Drawing.Imaging;

using AForge.Video;
using AForge.Video.DirectShow;
using OpenCvSharp;
using System.IO;



namespace ProcesamientoDeImágenes
{

    public partial class Camera : System.Windows.Window
    {
        private string selectedFilter = "None";
        private List<OpenCvSharp.Rect> lastDetectedFaces = new List<OpenCvSharp.Rect>();
        private int frameCounter = 0;

        private OpenCvSharp.Scalar selectedColor = new OpenCvSharp.Scalar(0, 0, 0);

        private CascadeClassifier faceCascade;

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        private System.Drawing.Bitmap lastDisplayedBitmap;

        private bool isFlipped = false;
        private bool isFaceDetectionActive = false; 

        public Camera()
        {
            InitializeComponent();
            LoadFaceCascade();
            LoadCameras();
        }

        private void LoadCameras()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count > 0)
            {
                CameraComboBox.ItemsSource = videoDevices.Cast<FilterInfo>().Select(d => d.Name).ToList();
                CameraComboBox.SelectedIndex = 0;
            }
        }
        private void LoadFaceCascade()
        {

            string cascadePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "haarcascade", "haarcascade_frontalface_default.xml");

            faceCascade = new CascadeClassifier(cascadePath);
        }
        private void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartCamera(CameraComboBox.SelectedIndex);
        }


        private void StartCamera(int cameraIndex)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= NewFrameReceived;
            }

            if (cameraIndex >= 0 && cameraIndex < videoDevices.Count)
            {
                videoSource = new VideoCaptureDevice(videoDevices[cameraIndex].MonikerString);
                videoSource.NewFrame += NewFrameReceived;
                videoSource.Start();
            }
        }

        private void NewFrameReceived(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Mat frame = OpenCvSharp.Extensions.BitmapConverter.ToMat(eventArgs.Frame);

                    frameCounter++;
                    if (isFaceDetectionActive && frameCounter % 5 == 0)
                    {
                        lastDetectedFaces = DetectFaces(frame);
                    }

                    Mat filteredFrame = ApplySelectedFilter(frame);

                    if (isFaceDetectionActive)
                    {
                        DrawDetectedFaces(filteredFrame, lastDetectedFaces);
                    }

                    // Convert to Bitmap only once
                    lastDisplayedBitmap?.Dispose();
                    lastDisplayedBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(filteredFrame);

                    BitmapSource bitmapSource = ConvertToBitmapSource(lastDisplayedBitmap);
                    CameraDisplay.Source = bitmapSource;

                    DrawHistograms(filteredFrame);

                    frame.Dispose();
                    filteredFrame.Dispose();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CameraDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (lastDisplayedBitmap == null) return;

            // Get click position relative to the image
            var position = e.GetPosition(CameraDisplay);

            // Scale position to bitmap size
            double xScale = lastDisplayedBitmap.Width / CameraDisplay.ActualWidth;
            double yScale = lastDisplayedBitmap.Height / CameraDisplay.ActualHeight;

            int x = (int)(position.X * xScale);
            int y = (int)(position.Y * yScale);

            // Bounds check
            if (x >= 0 && x < lastDisplayedBitmap.Width && y >= 0 && y < lastDisplayedBitmap.Height)
            {
                var color = lastDisplayedBitmap.GetPixel(x, y);
                string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                HexColorTextBox.Text = hex;
                HexColorTextBox.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));

            }
        }


        private BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private List<OpenCvSharp.Rect> DetectFaces(Mat frame)
        {
            List<OpenCvSharp.Rect> facesList = new List<OpenCvSharp.Rect>();

            if (faceCascade == null)
                return facesList;

            double scaleFactor = 0.5;
            Mat smallFrame = new Mat();
            Cv2.Resize(frame, smallFrame, new OpenCvSharp.Size(), scaleFactor, scaleFactor);

            Mat gray = new Mat();
            Cv2.CvtColor(smallFrame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);

            var faces = faceCascade.DetectMultiScale(
                gray,
                1.1,
                4,
                HaarDetectionTypes.ScaleImage,
                new OpenCvSharp.Size(50, 50)
            );

            foreach (var face in faces)
            {
                // Rescale to original size
                facesList.Add(new OpenCvSharp.Rect(
                    (int)(face.X / scaleFactor),
                    (int)(face.Y / scaleFactor),
                    (int)(face.Width / scaleFactor),
                    (int)(face.Height / scaleFactor)
                ));
            }

            smallFrame.Dispose();
            gray.Dispose();

            return facesList;
        }

        private void DrawDetectedFaces(Mat frame, List<OpenCvSharp.Rect> faces)
        {
            foreach (var face in faces)
            {
                Cv2.Rectangle(frame, face, Scalar.Red, 2);
            }
        }

        private void DrawHistograms(Mat frame)
        {
            if (frame.Empty())
                return;

            // Separate the frame into B, G, R channels
            Mat[] bgr = Cv2.Split(frame);

            int histSize = 256;
            Rangef histRange = new Rangef(0, 256);

            // Calculate histograms for each channel
            Mat bHist = new Mat();
            Mat gHist = new Mat();
            Mat rHist = new Mat();

            Cv2.CalcHist(new Mat[] { bgr[0] }, new int[] { 0 }, null, bHist, 1, new int[] { histSize }, new Rangef[] { histRange });
            Cv2.CalcHist(new Mat[] { bgr[1] }, new int[] { 0 }, null, gHist, 1, new int[] { histSize }, new Rangef[] { histRange });
            Cv2.CalcHist(new Mat[] { bgr[2] }, new int[] { 0 }, null, rHist, 1, new int[] { histSize }, new Rangef[] { histRange });

            // Normalize histograms to fit Canvas height
            int canvasHeight = 100; // Adjust based on your Canvas size
            Cv2.Normalize(bHist, bHist, 0, canvasHeight, NormTypes.MinMax);
            Cv2.Normalize(gHist, gHist, 0, canvasHeight, NormTypes.MinMax);
            Cv2.Normalize(rHist, rHist, 0, canvasHeight, NormTypes.MinMax);

            // Clear previous drawings
            HistogramRedCanvas.Children.Clear();
            HistogramGreenCanvas.Children.Clear();
            HistogramBlueCanvas.Children.Clear();

            // Draw histograms
            DrawHistogramOnCanvas(HistogramRedCanvas, rHist, System.Windows.Media.Brushes.Red);
            DrawHistogramOnCanvas(HistogramGreenCanvas, gHist, System.Windows.Media.Brushes.Green);
            DrawHistogramOnCanvas(HistogramBlueCanvas, bHist, System.Windows.Media.Brushes.Blue);

            // Dispose
            foreach (var mat in bgr)
                mat.Dispose();
            bHist.Dispose();
            gHist.Dispose();
            rHist.Dispose();
        }

        private void DrawHistogramOnCanvas(Canvas canvas, Mat hist, System.Windows.Media.Brush color)
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
            string url = "https://firebasestorage.googleapis.com/v0/b/studify-707e5/o/MID-picture-lab-manual-de-usuario.pdf?alt=media&token=b06e1bde-cadc-446f-a533-7dd3db592cd2";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void OnFlipImageIconClick(object sender, RoutedEventArgs e)
        {
            if (!isFlipped)
            {
                CameraDisplay.RenderTransform = new ScaleTransform
                {
                    ScaleX = -1,
                    ScaleY = 1
                };
            }
            else
            {
                CameraDisplay.RenderTransform = new ScaleTransform
                {
                    ScaleX = 1,
                    ScaleY = 1
                };
            }

            isFlipped = !isFlipped;
        }


        private void OnScreenShotIconClick(object sender, RoutedEventArgs e)
        {
            if (CameraDisplay.Source is BitmapImage bitmapImage)
            {
                SaveBitmapImage(bitmapImage);
            }
        }
        private void SaveBitmapImage(BitmapImage bitmapImage)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Screenshot",
                DefaultExt = ".png",
                Filter = "PNG Image (.png)|*.png|JPEG Image (.jpg)|*.jpg"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var fileStream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        private void OnFaceDetectIconClick(object sender, RoutedEventArgs e)
        {
            isFaceDetectionActive = !isFaceDetectionActive; 
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

                case "Color Mask":
                    filtered = ApplyColorMask(frame, selectedColor);
                    break;

                default:
                    filtered = frame.Clone();
                    break;
            }

            return filtered;
        }

        private void OnColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                var color = e.NewValue.Value;
                selectedColor = new OpenCvSharp.Scalar(color.B, color.G, color.R); 
            }
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
        private Mat ApplyColorMask(Mat frame, Scalar selectedColor)
        {
            Mat mask = new Mat();
            Mat result = new Mat();

            // Define a range around the selected color
            int threshold = 40; // you can adjust how sensitive it is

            Scalar lower = new Scalar(
                Math.Max(selectedColor.Val0 - threshold, 0),
                Math.Max(selectedColor.Val1 - threshold, 0),
                Math.Max(selectedColor.Val2 - threshold, 0));

            Scalar upper = new Scalar(
                Math.Min(selectedColor.Val0 + threshold, 255),
                Math.Min(selectedColor.Val1 + threshold, 255),
                Math.Min(selectedColor.Val2 + threshold, 255));

            // Create a mask of pixels within the color range
            Cv2.InRange(frame, lower, upper, mask);

            // Apply the mask to the original frame
            Cv2.BitwiseAnd(frame, frame, result, mask);

            mask.Dispose();
            return result;
        }

    }
}
