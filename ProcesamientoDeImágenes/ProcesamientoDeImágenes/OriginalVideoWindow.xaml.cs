using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Window = System.Windows.Window;
using System;

namespace ProcesamientoDeImágenes
{
    public partial class OriginalVideoWindow : Window
    {
        private VideoCapture capture;
        private CancellationTokenSource cts;

        public OriginalVideoWindow(string videoPath)
        {
            InitializeComponent();
            capture = new VideoCapture(videoPath);
            cts = new CancellationTokenSource();
            _ = Task.Run(() => PlayOriginalVideoAsync(cts.Token));
        }

        private async Task PlayOriginalVideoAsync(CancellationToken token)
        {
            var frame = new Mat();
            try
            {
                while (!token.IsCancellationRequested && capture.IsOpened())
                {
                    if (!capture.Read(frame) || frame.Empty())
                        break;

                    var bitmap = BitmapSourceConverter.ToBitmapSource(frame);
                    bitmap.Freeze();

                    Dispatcher.Invoke(() => OriginalImage.Source = bitmap);
                    await Task.Delay((int)(1000 / capture.Fps));
                }

                capture.Release();
            }
            finally
            {
                frame.Dispose();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            cts.Cancel();
            capture.Release();
            capture.Dispose();
        }
    }
}
