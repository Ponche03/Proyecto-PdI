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



        private bool isDragging = false;  // Flag to track if the image is being dragged
        private Point initialMousePosition;  // Store the initial position of the mouse
        private double initialX, initialY;  // Store the initial position of the image

        // Variables to track zoom
        private double zoomFactor = 1;  // Initial zoom factor
        private const double ZoomStep = 0.1;  // Zoom step

        private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Start dragging when mouse button is pressed
            isDragging = true;
            initialMousePosition = e.GetPosition(this);  // Get initial mouse position
            initialX = ImageTranslateTransform.X;  // Store the initial position of the image
            initialY = ImageTranslateTransform.Y;

            // Capture the mouse so we can track the movement even if it leaves the image
            UploadedImage.CaptureMouse();
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calculate the new position based on the mouse movement
                var mousePosition = e.GetPosition(this);
                double deltaX = mousePosition.X - initialMousePosition.X;
                double deltaY = mousePosition.Y - initialMousePosition.Y;

                // Update the translation of the image
                ImageTranslateTransform.X = initialX + deltaX;
                ImageTranslateTransform.Y = initialY + deltaY;
            }
        }

        private void OnImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Stop dragging when the mouse button is released
            isDragging = false;
            UploadedImage.ReleaseMouseCapture();  // Release the mouse capture
        }

        private void OnImageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
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

            
            ImageScaleTransform.ScaleX = zoomFactor;
            ImageScaleTransform.ScaleY = zoomFactor;

            double offsetX = mousePosition.X * ZoomStep;
            double offsetY = mousePosition.Y * ZoomStep;

            ImageTranslateTransform.X -= offsetX;
            ImageTranslateTransform.Y -= offsetY;

            
            AdjustImagePosition();

            UploadedImage.UpdateLayout();
        }

     
        private void AdjustImagePosition()
        {
            // Get the visible width and height of the ScrollViewer
            double scrollViewerWidth = ImageScrollViewer.ViewportWidth;
            double scrollViewerHeight = ImageScrollViewer.ViewportHeight;

            // Get the actual width and height of the image after scaling
            double imageWidth = UploadedImage.ActualWidth * ImageScaleTransform.ScaleX;
            double imageHeight = UploadedImage.ActualHeight * ImageScaleTransform.ScaleY;

            // Prevent the image from going out of bounds
            if (imageWidth < scrollViewerWidth)
            {
                ImageTranslateTransform.X = (scrollViewerWidth - imageWidth) / 2;
            }
            else
            {
                ImageTranslateTransform.X = Math.Max(Math.Min(ImageTranslateTransform.X, 0), -(imageWidth - scrollViewerWidth));
            }

            if (imageHeight < scrollViewerHeight)
            {
                ImageTranslateTransform.Y = (scrollViewerHeight - imageHeight) / 2;
            }
            else
            {
                ImageTranslateTransform.Y = Math.Max(Math.Min(ImageTranslateTransform.Y, 0), -(imageHeight - scrollViewerHeight));
            }
        }











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
                originalImage = new BitmapImage(new Uri(openFileDialog.FileName));
                UploadedImage.Source = originalImage;
                
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


        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedFilter = (comboBoxFilterSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedFilter != null)
            {
                ApplyFilter(selectedFilter);
            }
        }












        private void ApplyFilter(string filterName)
        {
            if(UploadedImage != null)
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
                    // Apply contrast adjustment
                    break;
                case "Sharpness Filter":
                    // Apply sharpening
                    break;
                case "Threshold Filter":
                    // Apply threshold filter
                    break;
                case "Hue/Saturation Filter":
                    // Modify hue and saturation
                    break;
                case "Negative Filter":
                    // Apply negative effect
                    break;
                case "Vignette Filter":
                    // Apply vignette effect
                    break;
                case "Mosaic Filter":
                        UploadedImage.Source = ApplyMosaicFilter(writableBitmap, 10);
                        break;
                case "Retro Effect":
                    // Apply retro effect
                    break;
                case "Warp Filter":
                    // Apply warp effect
                    break;
                default:
                    MessageBox.Show("Unknown filter selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            }


        }










        private void GenerateHistograms(BitmapSource image)
        {
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];
            image.CopyPixels(pixelData, stride, 0);

            int[] redHist = new int[256];
            int[] greenHist = new int[256];
            int[] blueHist = new int[256];

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                blueHist[pixelData[i]]++;
                greenHist[pixelData[i + 1]]++;
                redHist[pixelData[i + 2]]++;
            }

            DrawHistogram(RedHistogram, redHist, Brushes.Red);
            DrawHistogram(GreenHistogram, greenHist, Brushes.Green);
            DrawHistogram(BlueHistogram, blueHist, Brushes.Blue);
        }

        private void DrawHistogram(Canvas canvas, int[] histogram, Brush color)
        {
            canvas.Children.Clear();
            int max = histogram.Max();
            double scale = canvas.Height / (double)max;
            for (int i = 0; i < 256; i++)
            {
                Line line = new Line
                {
                    X1 = i,
                    Y1 = canvas.Height,
                    X2 = i,
                    Y2 = canvas.Height - (histogram[i] * scale),
                    Stroke = color,
                    StrokeThickness = 1
                };
                canvas.Children.Add(line);
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



    }

}
