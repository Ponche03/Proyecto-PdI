using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private BitmapImage originalImage;
        private bool isDragging = false;
        private Point initialMousePosition;
        private double initialX, initialY;
        private double zoomFactor = 1;
        private const double ZoomStep = 0.1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging of the image even if the mouse is clicked inside the container
            if (e.OriginalSource == UploadedImage || e.OriginalSource == ImageScrollViewer)
            {
                isDragging = true;
                initialMousePosition = e.GetPosition(this);
                initialX = ImageTranslateTransform.X;
                initialY = ImageTranslateTransform.Y;

                UploadedImage.CaptureMouse();
            }
        }
        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var mousePosition = e.GetPosition(this);
                double deltaX = mousePosition.X - initialMousePosition.X;
                double deltaY = mousePosition.Y - initialMousePosition.Y;

                ImageTranslateTransform.X = initialX + deltaX;
                ImageTranslateTransform.Y = initialY + deltaY;
            }
        }
        private void OnImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            UploadedImage.ReleaseMouseCapture();
        }
        private void OnImageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom in or out depending on the scroll direction
            if (e.Delta > 0)
            {
                zoomFactor += ZoomStep;
            }
            else if (e.Delta < 0)
            {
                zoomFactor -= ZoomStep;
                if (zoomFactor < ZoomStep) zoomFactor = ZoomStep;
            }

            Point mousePosition = e.GetPosition(UploadedImage);

            // Apply scaling to the image
            ImageScaleTransform.ScaleX = zoomFactor;
            ImageScaleTransform.ScaleY = zoomFactor;

            // Adjust translation to zoom around the mouse pointer
            double offsetX = mousePosition.X * ZoomStep;
            double offsetY = mousePosition.Y * ZoomStep;

            ImageTranslateTransform.X -= offsetX;
            ImageTranslateTransform.Y -= offsetY;

            AdjustImagePosition();

            UploadedImage.UpdateLayout();
        }
        private void AdjustImagePosition()
        {
            // Get the dimensions of the image and the scroll viewer
            double scrollViewerWidth = ImageScrollViewer.ViewportWidth;
            double scrollViewerHeight = ImageScrollViewer.ViewportHeight;

            double imageWidth = UploadedImage.ActualWidth * ImageScaleTransform.ScaleX;
            double imageHeight = UploadedImage.ActualHeight * ImageScaleTransform.ScaleY;

            // Center the image if it's smaller than the ScrollViewer
            if (imageWidth < scrollViewerWidth)
            {
                ImageTranslateTransform.X = (scrollViewerWidth - imageWidth) / 2;
            }
            else
            {
                ImageTranslateTransform.X = Math.Max(Math.Min(ImageTranslateTransform.X, 0), -(imageWidth - scrollViewerWidth));
            }

            // Center the image vertically if it's smaller than the ScrollViewer
            if (imageHeight < scrollViewerHeight)
            {
                ImageTranslateTransform.Y = (scrollViewerHeight - imageHeight) / 2;
            }
            else
            {
                ImageTranslateTransform.Y = Math.Max(Math.Min(ImageTranslateTransform.Y, 0), -(imageHeight - scrollViewerHeight));
            }
        }
        private void OnImageMouseDownDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Open a new window with the image on double-click
            if (e.ClickCount == 2)
            {
                if (UploadedImage.Source is BitmapSource bitmapSource)
                {
                    var imageWindow = new ImageWindow(bitmapSource);
                    imageWindow.Show();
                }
            }
        }


        private void GoToImageWindow(object sender, RoutedEventArgs e)
        {

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
            Camera newWindow = new Camera();
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
            MessageBox.Show("Software version: 1.000", "", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void OnUploadImageIconClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select an Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
           
                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                WriteableBitmap writableBitmap = new WriteableBitmap(bitmapImage);
                UploadedImage.Source = writableBitmap;

                originalImage = bitmapImage;
                UpdateHistogram(writableBitmap);

            }

        }
        private void OnDownloadImageIconClick(object sender, RoutedEventArgs e)
        {
            if (UploadedImage.Source is BitmapSource bitmapSource)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                    Title = "Save Edited Image",
                    FileName = "EditedImage"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        BitmapEncoder encoder;

                        // Choose encoder based on file extension
                        string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                        switch (extension)
                        {
                            case ".jpg":
                            case ".jpeg":
                                encoder = new JpegBitmapEncoder();
                                break;
                            case ".bmp":
                                encoder = new BmpBitmapEncoder();
                                break;
                            default:
                                encoder = new PngBitmapEncoder();
                                break;
                        }

                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("Image saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("No image to save!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


       
        

        private void UpdateHistogram(WriteableBitmap bitmap)
        {
            int[] redHistogram = new int[256];
            int[] greenHistogram = new int[256];
            int[] blueHistogram = new int[256];

            // Compute histograms for each channel
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    Color pixelColor = GetPixelColor(bitmap, x, y);
                    redHistogram[pixelColor.R]++;
                    greenHistogram[pixelColor.G]++;
                    blueHistogram[pixelColor.B]++;
                }
            }

            // Draw histograms for each channel
            DrawHistogram(RedHistogram, redHistogram);
            DrawHistogram(GreenHistogram, greenHistogram);
            DrawHistogram(BlueHistogram, blueHistogram);
        }
        private void DrawHistogram(Canvas canvas, int[] histogram)
        {
            // Ensure canvas size is valid
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
            {
                return; // Exit if canvas size is invalid or zero
            }

            double maxHeight = canvasHeight;

            // Clear any previous drawings
            canvas.Children.Clear();

            int maxValue = histogram.Max();
            if (maxValue == 0) return;  // No data to draw

            // Scale the histogram values
            for (int i = 0; i < 256; i++)
            {
                double barHeight = (histogram[i] / (double)maxValue) * maxHeight;

                // Create and position a rectangle for each value in the histogram
                var bar = new System.Windows.Shapes.Rectangle
                {
                    Width = canvasWidth / 256, // 256 bins for histogram
                    Height = barHeight,
                    Fill = GetColorForHistogram(canvas, i),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(i * (canvasWidth / 256), 0, 0, 0)
                };

                canvas.Children.Add(bar);
            }
        }
        private Brush GetColorForHistogram(Canvas canvas, int index)
        {
            if (canvas == RedHistogram)
                return Brushes.Red;
            else if (canvas == GreenHistogram)
                return Brushes.Green;
            else if (canvas == BlueHistogram)
                return Brushes.Blue;
            return Brushes.Gray;
        }
        private Color GetPixelColor(WriteableBitmap bitmap, int x, int y)
        {
            // Get the stride (width * 4 bytes per pixel)
            int stride = bitmap.PixelWidth * 4;

            // Create a byte array to hold the pixel data for the specific row
            byte[] pixelData = new byte[4]; // 4 bytes per pixel (BGRA)

            // Copy the pixels at (x, y) into the byte array
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixelData, stride, 0);

            // Return the color in BGRA format, we convert it to ARGB (A, R, G, B)
            return Color.FromArgb(pixelData[3], pixelData[2], pixelData[1], pixelData[0]); // A, R, G, B
        }

        private void OnFilterButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string filterName = button.Content.ToString();

                if (UploadedImage != null)
                {

                    if (UploadedImage.Source == null)
                    {
                        MessageBox.Show("Please upload an image first!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    BitmapSource bitmapSource = UploadedImage.Source as BitmapSource;
                    if (bitmapSource == null) return;

                    WriteableBitmap writableBitmap = new WriteableBitmap(bitmapSource);

                    switch (filterName)
                    {
                        case "None":
                            UploadedImage.Source = originalImage;
                            break;
                        case "Gaussian Blur":
                            UploadedImage.Source = ApplyGaussianBlur(writableBitmap, 10);
                            break;
                        case "Contrast Filter":
                            UploadedImage.Source = ApplyContrastFilter(writableBitmap, 50);
                            break;
                        case "Sharpness Filter":
                            UploadedImage.Source = ApplySharpnessFilter(writableBitmap);
                            break;
                        case "Threshold Filter":
                            UploadedImage.Source = ApplyThresholdFilter(writableBitmap, 128);
                            break;
                        case "Hue/Saturation Filter":
                            double hueShift = 30.0;
                            double saturationFactor = 1.5;
                            UploadedImage.Source = ApplyHueSaturationFilter(writableBitmap, hueShift, saturationFactor);
                            break;
                        case "Negative Filter":
                            UploadedImage.Source = ApplyNegativeFilter(writableBitmap);
                            break;
                        case "Vignette Filter":
                            UploadedImage.Source = ApplyVignetteFilter(writableBitmap);
                            break;
                        case "Mosaic Filter":
                            UploadedImage.Source = ApplyMosaicFilter(writableBitmap, 10);
                            break;
                        case "Retro Effect":
                            UploadedImage.Source = ApplyRetroEffect(writableBitmap);
                            break;
                        case "Warp Filter":
                            UploadedImage.Source = ApplyWarpEffect(writableBitmap, 0.5);
                            break;
                        default:
                            MessageBox.Show("Unknown filter selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
        }

        private WriteableBitmap ApplyGaussianBlur(WriteableBitmap bitmap, int radius)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4;

            byte[] pixels = new byte[height * stride];
            bitmap.CopyPixels(pixels, stride, 0);

            // Gaussian Kernel for 3x3 blur
            double[,] kernel = {
                 { 1, 2, 1 },
                 { 2, 4, 2 },
                 { 1, 2, 1 }
             };

            // int kernelSize = 3;
            double kernelSum = 16.0; 

            byte[] blurredPixels = new byte[pixels.Length];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double blue = 0, green = 0, red = 0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int pixelIndex = ((y + ky) * stride) + ((x + kx) * 4);
                            double kernelValue = kernel[ky + 1, kx + 1];

                            blue += pixels[pixelIndex] * kernelValue;
                            green += pixels[pixelIndex + 1] * kernelValue;
                            red += pixels[pixelIndex + 2] * kernelValue;
                        }
                    }

                    int index = (y * stride) + (x * 4);
                    blurredPixels[index] = (byte)(blue / kernelSum);
                    blurredPixels[index + 1] = (byte)(green / kernelSum);
                    blurredPixels[index + 2] = (byte)(red / kernelSum);
                    blurredPixels[index + 3] = pixels[index + 3]; 
                }
            }

            WriteableBitmap blurredBitmap = new WriteableBitmap(width, height, 96, 96, bitmap.Format, null);
            blurredBitmap.WritePixels(new Int32Rect(0, 0, width, height), blurredPixels, stride, 0);

            return blurredBitmap;
        }

        private BitmapSource ApplyMosaicFilter(WriteableBitmap bitmap, int blockSize)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            for (int y = 0; y < height; y += blockSize)
            {
                for (int x = 0; x < width; x += blockSize)
                {
                    int redSum = 0, greenSum = 0, blueSum = 0, count = 0;

                    // Calculate average color in block
                    for (int j = 0; j < blockSize && (y + j) < height; j++)
                    {
                        for (int i = 0; i < blockSize && (x + i) < width; i++)
                        {
                            int pixelIndex = ((y + j) * stride) + ((x + i) * 4);
                            blueSum += pixelData[pixelIndex];     // Blue
                            greenSum += pixelData[pixelIndex + 1]; // Green
                            redSum += pixelData[pixelIndex + 2];   // Red
                            count++;
                        }
                    }

                    if (count == 0) continue;

                    byte avgBlue = (byte)(blueSum / count);
                    byte avgGreen = (byte)(greenSum / count);
                    byte avgRed = (byte)(redSum / count);

                    // Apply average color to block
                    for (int j = 0; j < blockSize && (y + j) < height; j++)
                    {
                        for (int i = 0; i < blockSize && (x + i) < width; i++)
                        {
                            int pixelIndex = ((y + j) * stride) + ((x + i) * 4);
                            pixelData[pixelIndex] = avgBlue;      // Set Blue
                            pixelData[pixelIndex + 1] = avgGreen; // Set Green
                            pixelData[pixelIndex + 2] = avgRed;   // Set Red
                        }
                    }
                }
            }

            // Create new bitmap from modified pixels
            WriteableBitmap mosaicBitmap = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight,
                                                                bitmap.DpiX, bitmap.DpiY,
                                                                bitmap.Format, bitmap.Palette);
            mosaicBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return mosaicBitmap;
        }

        private BitmapSource ApplyContrastFilter(WriteableBitmap bitmap, double contrastFactor)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Calculate contrast adjustment factor
            double contrast = (100.0 + contrastFactor) / 100.0;
            contrast *= contrast;

            for (int i = 0; i < pixelData.Length; i += 4) // Iterate through each pixel (BGRA)
            {
                for (int j = 0; j < 3; j++) // Modify only R, G, and B channels
                {
                    double color = pixelData[i + j] / 255.0; // Normalize to (0 to 1)
                    color -= 0.5;  // Shift to center at 0
                    color *= contrast; // Apply contrast factor
                    color += 0.5;  // Shift back
                    color *= 255.0; // Convert back to range (0 to 255)

                    // Ensure the values are within the valid range
                    pixelData[i + j] = (byte)Math.Max(0, Math.Min(255, color));
                }
            }

            // Create a new image with the modified pixels
            WriteableBitmap contrastBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            contrastBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return contrastBitmap;
        }

        private BitmapSource ApplySharpnessFilter(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            byte[] resultPixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Sharpening kernel (3x3)
            int[,] kernel = {
             {  0, -1,  0 },
             { -1,  5, -1 },
             {  0, -1,  0 }
             };

            int kernelSize = 3;
            int offset = kernelSize / 2;

            for (int y = offset; y < height - offset; y++)
            {
                for (int x = offset; x < width - offset; x++)
                {
                    int pixelIndex = (y * stride) + (x * 4);

                    double[] newColor = { 0, 0, 0 }; // For R, G, B

                    // Apply the kernel to each color channel
                    for (int ky = -offset; ky <= offset; ky++)
                    {
                        for (int kx = -offset; kx <= offset; kx++)
                        {
                            int neighborIndex = ((y + ky) * stride) + ((x + kx) * 4);
                            int weight = kernel[ky + offset, kx + offset];

                            for (int c = 0; c < 3; c++) // Process R, G, B channels
                            {
                                newColor[c] += pixelData[neighborIndex + c] * weight;
                            }
                        }
                    }

                    // Assign the new sharpened values to the result pixel
                    for (int c = 0; c < 3; c++)
                    {
                        resultPixelData[pixelIndex + c] = (byte)Math.Max(0, Math.Min(255, newColor[c]));
                    }

                    // Preserve the alpha channel
                    resultPixelData[pixelIndex + 3] = pixelData[pixelIndex + 3];
                }
            }

            // Create a new bitmap with the sharpened pixels
            WriteableBitmap sharpBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            sharpBitmap.WritePixels(new Int32Rect(0, 0, width, height), resultPixelData, stride, 0);

            return sharpBitmap;
        }

        private BitmapSource ApplyThresholdFilter(WriteableBitmap bitmap, byte threshold)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i += 4) // Iterate over each pixel (BGRA)
            {
                // Convert the pixel to grayscale using the luminance formula
                byte gray = (byte)(0.299 * pixelData[i + 2] + 0.587 * pixelData[i + 1] + 0.114 * pixelData[i]);

                // Apply thresholding: if the grayscale value is greater than the threshold, set to white; otherwise, set to black
                byte newColor = (gray >= threshold) ? (byte)255 : (byte)0;

                // Set R, G, and B channels to the new threshold color
                pixelData[i] = newColor;      // Blue
                pixelData[i + 1] = newColor;  // Green
                pixelData[i + 2] = newColor;  // Red
            }

            // Create a new bitmap with the thresholded pixels
            WriteableBitmap thresholdBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            thresholdBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return thresholdBitmap;
        }

        private BitmapSource ApplyHueSaturationFilter(WriteableBitmap bitmap, double hueShift, double saturationFactor)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i += 4) // Iterate over each pixel (BGRA)
            {
                byte b = pixelData[i];     // Blue
                byte g = pixelData[i + 1]; // Green
                byte r = pixelData[i + 2]; // Red

                // Convert RGB to HSL
                double rNorm = r / 255.0;
                double gNorm = g / 255.0;
                double bNorm = b / 255.0;

                double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
                double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
                double delta = max - min;

                double hue = 0.0;
                if (delta != 0)
                {
                    if (max == rNorm) hue = (gNorm - bNorm) / delta;
                    else if (max == gNorm) hue = 2 + (bNorm - rNorm) / delta;
                    else hue = 4 + (rNorm - gNorm) / delta;

                    hue *= 60.0;
                    if (hue < 0) hue += 360.0;
                }

                double lightness = (max + min) / 2.0;
                double saturation = (max == min) ? 0.0 : (max - min) / (1 - Math.Abs(2 * lightness - 1));

                // Apply the hue shift
                hue += hueShift;
                if (hue > 360.0) hue -= 360.0;
                if (hue < 0.0) hue += 360.0;

                // Apply the saturation adjustment
                saturation = Math.Max(0.0, Math.Min(1.0, saturation * saturationFactor));

                // Convert HSL back to RGB
                double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
                double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
                double m = lightness - c / 2.0;

                double rPrime = 0.0, gPrime = 0.0, bPrime = 0.0;

                if (hue >= 0 && hue < 60) { rPrime = c; gPrime = x; bPrime = 0; }
                else if (hue >= 60 && hue < 120) { rPrime = x; gPrime = c; bPrime = 0; }
                else if (hue >= 120 && hue < 180) { rPrime = 0; gPrime = c; bPrime = x; }
                else if (hue >= 180 && hue < 240) { rPrime = 0; gPrime = x; bPrime = c; }
                else if (hue >= 240 && hue < 300) { rPrime = x; gPrime = 0; bPrime = c; }
                else { rPrime = c; gPrime = 0; bPrime = x; }

                // Adjust by the m value (offset)
                rPrime += m;
                gPrime += m;
                bPrime += m;

                // Convert RGB back to byte values
                byte newR = (byte)(Math.Max(0, Math.Min(255, rPrime * 255)));
                byte newG = (byte)(Math.Max(0, Math.Min(255, gPrime * 255)));
                byte newB = (byte)(Math.Max(0, Math.Min(255, bPrime * 255)));

                // Update the pixel data with the new RGB values
                pixelData[i] = newB;
                pixelData[i + 1] = newG;
                pixelData[i + 2] = newR;
            }

            // Create a new WriteableBitmap with the modified pixels
            WriteableBitmap hueSaturationBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            hueSaturationBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return hueSaturationBitmap;
        }

        private BitmapSource ApplyNegativeFilter(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i += 4) // Iterate over each pixel (BGRA)
            {
                // Invert Red, Green, and Blue channels
                pixelData[i] = (byte)(255 - pixelData[i]);     // Blue
                pixelData[i + 1] = (byte)(255 - pixelData[i + 1]); // Green
                pixelData[i + 2] = (byte)(255 - pixelData[i + 2]); // Red
            }

            // Create a new bitmap with modified pixels
            WriteableBitmap negativeBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            negativeBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return negativeBitmap;
        }

        private BitmapSource ApplyVignetteFilter(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            double centerX = width / 2.0;
            double centerY = height / 2.0;
            double maxDistance = Math.Sqrt(centerX * centerX + centerY * centerY); // Maximum distance from center

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (y * stride) + (x * 4);
                    double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));

                    // Calculate vignette strength (distance effect)
                    double vignetteStrength = 1 - (distance / maxDistance); // Value between 0 and 1

                    // Apply vignette effect by reducing brightness based on distance
                    for (int channel = 0; channel < 3; channel++) // Red, Green, Blue channels
                    {
                        pixelData[pixelIndex + channel] = (byte)(pixelData[pixelIndex + channel] * vignetteStrength);
                    }
                }
            }

            // Create a new bitmap with the modified pixels
            WriteableBitmap vignetteBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            vignetteBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return vignetteBitmap;
        }

        private BitmapSource ApplyRetroEffect(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Apply desaturation to make the image more faded (not full black and white)
            double saturationFactor = 0.6; // A value between 0 (grayscale) and 1 (full color)

            // Apply a warm brownish tint (similar to old photos)
            byte tintRed = 70, tintGreen = 50, tintBlue = 30; // Brownish tint

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                // Extract RGB values
                byte red = pixelData[i + 2];
                byte green = pixelData[i + 1];
                byte blue = pixelData[i];

                // Desaturate the color by calculating the average and blending with the original color
                byte gray = (byte)((red + green + blue) / 3);

                red = (byte)(gray + (red - gray) * saturationFactor);
                green = (byte)(gray + (green - gray) * saturationFactor);
                blue = (byte)(gray + (blue - gray) * saturationFactor);

                // Apply the brownish tint to create a retro/vintage look
                red = (byte)Math.Min(255, red + tintRed);
                green = (byte)Math.Min(255, green + tintGreen);
                blue = (byte)Math.Min(255, blue + tintBlue);

                // Apply the modified colors back to the pixel data
                pixelData[i + 2] = red; // Red channel
                pixelData[i + 1] = green; // Green channel
                pixelData[i] = blue; // Blue channel
            }

            // Create a new WriteableBitmap with the modified pixels
            WriteableBitmap retroBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            retroBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return retroBitmap;
        }

        public static WriteableBitmap ApplyWarpEffect(WriteableBitmap bitmap, double warpStrength)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            WriteableBitmap warpedBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette);
            byte[] warpedData = new byte[height * stride];

            int centerX = width / 2;
            int centerY = height / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double factor = 1 + warpStrength * Math.Sin(distance / 30);
                    int srcX = (int)(centerX + dx * factor);
                    int srcY = (int)(centerY + dy * factor);

                    if (srcX >= 0 && srcX < width && srcY >= 0 && srcY < height)
                    {
                        int srcIndex = (srcY * stride) + (srcX * 4);
                        int destIndex = (y * stride) + (x * 4);

                        warpedData[destIndex] = pixelData[srcIndex];       // Blue
                        warpedData[destIndex + 1] = pixelData[srcIndex + 1]; // Green
                        warpedData[destIndex + 2] = pixelData[srcIndex + 2]; // Red
                        warpedData[destIndex + 3] = pixelData[srcIndex + 3]; // Alpha
                    }
                }
            }

            warpedBitmap.WritePixels(new Int32Rect(0, 0, width, height), warpedData, stride, 0);
            return warpedBitmap;
        }

    }

}
