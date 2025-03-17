using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Input;
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

        // Use default.png if no valid crosshair is specified or if file doesn't exist.
        if (string.IsNullOrEmpty(crosshairPath) || !File.Exists(crosshairPath))
        {
            crosshairPath = Path.Combine(crosshairDir, "default.png");
        }

        // If the file exists, load it using a FileStream with CacheOption.OnLoad.
        if (File.Exists(crosshairPath))
        {
            BitmapImage bitmap = new BitmapImage();
            using (var stream = new FileStream(crosshairPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze(); // Optional: freeze for thread-safety.
            CrosshairImage.Source = bitmap;
            this.Width = bitmap.PixelWidth;
            this.Height = bitmap.PixelHeight;
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
    }
}