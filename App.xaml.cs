using System;
using System.IO;  // Add this line at the top
using System.Threading;
using System.Windows; // This is for WPF Application
using WinForms = System.Windows.Forms; // Alias for Windows Forms

namespace CrosshairOverlayApp
{
    public partial class App : Application // This refers to System.Windows.Application
    {
        private WinForms.NotifyIcon _trayIcon;
        public CrosshairOverlay CrosshairOverlay { get; private set; }
        private static Mutex _mutex = new Mutex(true, "{B1A2C3D4-E5F6-7890-1234-56789ABCDEF0}");

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Another instance is already running.", "Instance Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            try
            {
                Console.WriteLine("Application starting...");

                // Initialize system tray icon using TrayIcon.ico
                _trayIcon = new WinForms.NotifyIcon
                {
                    Icon = new System.Drawing.Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TrayIcon.ico")),
                    Text = "CrosshairOverlay",
                    Visible = true
                };

                // Create context menu for tray icon
                var contextMenu = new WinForms.ContextMenuStrip();
                var settingsMenuItem = new WinForms.ToolStripMenuItem("Settings");
                settingsMenuItem.Click += (s, ev) => new CrosshairSettings().Show();
                contextMenu.Items.Add(settingsMenuItem);
                var exitMenuItem = new WinForms.ToolStripMenuItem("Exit");
                exitMenuItem.Click += (s, ev) => ExitApp();
                contextMenu.Items.Add(exitMenuItem);
                _trayIcon.ContextMenuStrip = contextMenu;

                // Create and show the overlay window
                CrosshairOverlay = new CrosshairOverlay();
                CrosshairOverlay.Show();

                Console.WriteLine("Overlay window shown.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitApp();
            }
        }

        private void ExitApp()
        {
            _trayIcon?.Dispose();
            CrosshairOverlay?.Close();
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
            _mutex.ReleaseMutex();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Console.WriteLine($"Unhandled exception: {ex.Message}");
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Dispatcher exception: {e.Exception.Message}");
            e.Handled = true;
        }
    }
}