using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace CrosshairOverlayApp
{
    public partial class CrosshairOverlay : Window
    {
        private readonly string crosshairDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "crosshairs");
        private readonly string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

        public CrosshairOverlay()
        {
            InitializeComponent();
            // Set window icon programmatically using AppIcon.ico
            try
            {
                Uri iconUri = new Uri("pack://application:,,,/Resources/AppIcon.ico", UriKind.Absolute);
                this.Icon = BitmapFrame.Create(iconUri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading icon: " + ex.Message);
            }

            Console.WriteLine("Overlay initialized.");
            LoadCrosshair();
            PositionWindow();
            HideCursorWhenActive();

            // Attach the Loaded event so we can update the window style for click-through afterward.
            this.Loaded += (s, e) => MakeWindowClickThrough();
        }

        public void LoadCrosshair()
        {
            try
            {
                // Ensure the crosshair directory exists.
                if (!Directory.Exists(crosshairDir))
                {
                    Console.WriteLine($"Crosshair folder not found. Creating directory: {crosshairDir}");
                    Directory.CreateDirectory(crosshairDir);
                }

                // Read the crosshair path from settings (if available).
                string crosshairPath = "";
                if (File.Exists(settingsFile))
                {
                    crosshairPath = File.ReadAllText(settingsFile).Trim();
                }

                // Use default.png if no valid crosshair is specified or if the file doesn't exist.
                if (string.IsNullOrEmpty(crosshairPath) || !File.Exists(crosshairPath))
                {
                    crosshairPath = Path.Combine(crosshairDir, "default.png");
                }

                if (File.Exists(crosshairPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    // Load image using its URI instead of a stream.
                    bitmap.UriSource = new Uri(crosshairPath, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze(); // Freeze for thread-safety.

                    // Set the image source and force no stretching.
                    CrosshairImage.Source = bitmap;
                    CrosshairImage.Stretch = System.Windows.Media.Stretch.None;
                    // Use nearest neighbor scaling for a crisp image.
                    RenderOptions.SetBitmapScalingMode(CrosshairImage, BitmapScalingMode.NearestNeighbor);
                    CrosshairImage.UseLayoutRounding = true;
                    CrosshairImage.SnapsToDevicePixels = true;

                    // Retrieve DPI factors from the current visual.
                    double dpiX = 1.0, dpiY = 1.0;
                    var source = PresentationSource.FromVisual(this);
                    if (source != null)
                    {
                        dpiX = source.CompositionTarget.TransformToDevice.M11;
                        dpiY = source.CompositionTarget.TransformToDevice.M22;
                    }

                    // Adjust window size so that the crosshair is displayed at its native pixel dimensions.
                    this.Width = bitmap.PixelWidth / dpiX;
                    this.Height = bitmap.PixelHeight / dpiY;
                    PositionWindow();
                    Console.WriteLine("Crosshair loaded successfully.");
                }
                else
                {
                    Console.WriteLine("No crosshairs found in " + crosshairDir);
                    MessageBox.Show("No crosshairs found. Please add at least one crosshair image (e.g., default.png) in the Resources\\crosshairs folder.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading crosshair: {ex.Message}");
                MessageBox.Show($"Error loading crosshair: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PositionWindow()
        {
            try
            {
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
                Console.WriteLine("Window positioned.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error positioning window: {ex.Message}");
                MessageBox.Show($"Error positioning window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HideCursorWhenActive()
        {
            try
            {
                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += (s, e) =>
                {
                    this.Cursor = this.IsActive ? Cursors.None : Cursors.Arrow;
                };
                timer.Start();
                Console.WriteLine("Cursor hiding initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing cursor hiding: {ex.Message}");
                MessageBox.Show($"Error initializing cursor hiding: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the window’s extended style to include WS_EX_LAYERED and WS_EX_TRANSPARENT.
        /// This makes the window click-through while keeping it visible.
        /// </summary>
        private void MakeWindowClickThrough()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            IntPtr hwnd = helper.Handle;
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
                extendedStyle | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT);
        }
    }

    internal static class NativeMethods
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_LAYERED = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    }
}