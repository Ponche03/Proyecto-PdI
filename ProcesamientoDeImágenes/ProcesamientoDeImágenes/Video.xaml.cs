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
using System.Windows.Shapes;

namespace ProcesamientoDeImágenes
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : Window
    {
        private Uri originalVideoSource;
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

        private void OnUploadVideoIconClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.mkv",
                Title = "Select a Video File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                VideoPlayer.Source = new Uri(openFileDialog.FileName);
                originalVideoSource = VideoPlayer.Source;
                VideoPlayer.Play();
            }
        }

        private void OnDownloadVideoIconClick(object sender, RoutedEventArgs e)
        {
            // Handle video download functionality here
        }


        private void PlayVideo(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Play();
        }

        private void PauseVideo(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Pause();
        }

        private void StopVideo(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
        }


        private void OnFilterButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string filterName = button.Content.ToString();

                if (VideoPlayer != null && VideoPlayer.Source != null)
                {
                    // Capture the current video frame
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)VideoPlayer.ActualWidth, (int)VideoPlayer.ActualHeight,
                        96, 96, PixelFormats.Pbgra32);

                    renderBitmap.Render(VideoPlayer);

                    // Convert the captured frame to a WriteableBitmap
                    WriteableBitmap writableBitmap = new WriteableBitmap(renderBitmap);

                    switch (filterName)
                    {
                        case "None":
                            // No filter, just reset the video
                            VideoPlayer.Source = originalVideoSource;
                            break;

                        case "Gaussian Blur":
                            // Declare radius for Gaussian blur filter
                            int blurRadius = 10;
                            var blurredFrame = ApplyGaussianBlur(writableBitmap, blurRadius);
                            DisplayFilteredFrame(blurredFrame); // Display the filtered frame
                            break;

                        case "Contrast Filter":
                            // Declare contrast level for Contrast filter
                            int contrastLevel = 50;
                            var contrastFrame = ApplyContrastFilter(writableBitmap, contrastLevel);
                            DisplayFilteredFrame(contrastFrame); // Display the filtered frame
                            break;

                        case "Sharpness Filter":
                            var sharpnessFrame = ApplySharpnessFilter(writableBitmap);
                            DisplayFilteredFrame(sharpnessFrame); // Display the filtered frame
                            break;

                        case "Threshold Filter":
                            // Declare threshold level
                            int thresholdLevel = 128;
                            var thresholdFrame = ApplyThresholdFilter(writableBitmap, thresholdLevel);
                            DisplayFilteredFrame(thresholdFrame); // Display the filtered frame
                            break;

                        case "Hue/Saturation Filter":
                            // Declare variables for Hue and Saturation shift
                            double hueShift = 30.0;
                            double saturationFactor = 1.5;
                            var hueSaturationFrame = ApplyHueSaturationFilter(writableBitmap, hueShift, saturationFactor);
                            DisplayFilteredFrame(hueSaturationFrame); // Display the filtered frame
                            break;

                        case "Negative Filter":
                            // Apply negative filter
                            var negativeFrame = ApplyNegativeFilter(writableBitmap);
                            DisplayFilteredFrame(negativeFrame); // Display the filtered frame
                            break;

                        case "Vignette Filter":
                            // Apply vignette filter
                            var vignetteFrame = ApplyVignetteFilter(writableBitmap);
                            DisplayFilteredFrame(vignetteFrame); // Display the filtered frame
                            break;

                        case "Mosaic Filter":
                            // Declare mosaic size
                            int mosaicSize = 10;
                            var mosaicFrame = ApplyMosaicFilter(writableBitmap, mosaicSize);
                            DisplayFilteredFrame(mosaicFrame); // Display the filtered frame
                            break;

                        case "Retro Effect":
                            // Apply retro effect
                            var retroFrame = ApplyRetroEffect(writableBitmap);
                            DisplayFilteredFrame(retroFrame); // Display the filtered frame
                            break;

                        case "Warp Filter":
                            // Declare warp factor
                            double warpFactor = 0.5;
                            var warpFrame = ApplyWarpEffect(writableBitmap, warpFactor);
                            DisplayFilteredFrame(warpFrame); // Display the filtered frame
                            break;

                        default:
                            MessageBox.Show("Unknown filter selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("No video loaded. Please upload a video first.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DisplayFilteredFrame(BitmapSource filteredFrame) {
            // Assuming you have an Image control to display the filtered frame
            FilteredImage.Source = filteredFrame;
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

        private WriteableBitmap ApplyThresholdFilter(WriteableBitmap bitmap, int threshold)
        {
            // Ensure threshold is within byte range using the manual Clamp method
            byte thresholdByte = Clamp(threshold, 0, 255);

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4; // For RGBA

            byte[] pixels = new byte[height * stride];
            bitmap.CopyPixels(pixels, stride, 0);

            byte[] thresholdedPixels = new byte[pixels.Length];

            // Loop through each pixel and apply the threshold filter
            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Get the RGB values
                byte red = pixels[i + 2];
                byte green = pixels[i + 1];
                byte blue = pixels[i];

                // Convert to grayscale using standard luminance formula
                byte gray = (byte)((red * 0.3) + (green * 0.59) + (blue * 0.11));

                // Apply threshold (comparison works with byte values)
                byte newColor = (gray >= thresholdByte) ? (byte)255 : (byte)0;

                // Set the thresholded color (no change to alpha)
                thresholdedPixels[i] = newColor;      // Blue
                thresholdedPixels[i + 1] = newColor;  // Green
                thresholdedPixels[i + 2] = newColor;  // Red
                thresholdedPixels[i + 3] = pixels[i + 3]; // Alpha (unchanged)
            }

            // Create a new WriteableBitmap to display the filtered result
            WriteableBitmap thresholdedBitmap = new WriteableBitmap(width, height, 96, 96, bitmap.Format, null);
            thresholdedBitmap.WritePixels(new Int32Rect(0, 0, width, height), thresholdedPixels, stride, 0);

            return thresholdedBitmap;
        }


        private byte Clamp(int value, byte min, byte max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return (byte)value;
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
