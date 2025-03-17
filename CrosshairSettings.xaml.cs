using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using Microsoft.VisualBasic; // For Interaction.InputBox

namespace CrosshairOverlayApp
{
    public class CrosshairItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public partial class CrosshairSettings : Window
    {
        private readonly string crosshairDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "crosshairs");
        private readonly string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

        public CrosshairSettings()
        {
            InitializeComponent();
            LoadCrosshairs();
        }

        private void LoadCrosshairs()
        {
            try
            {
                if (!Directory.Exists(crosshairDir))
                {
                    Console.WriteLine($"Directory not found. Creating: {crosshairDir}");
                    Directory.CreateDirectory(crosshairDir);
                }

                // Clear existing items in both lists.
                PreloadedList.Items.Clear();
                PersonalList.Items.Clear();

                var crosshairFiles = Directory.GetFiles(crosshairDir, "*.png");
                foreach (var file in crosshairFiles)
                {
                    string fileName = Path.GetFileName(file);
                    var item = new CrosshairItem { FileName = fileName, FilePath = file };

                    // Preloaded crosshairs are determined by specific names.
                    if (fileName.Equals("default.png", StringComparison.InvariantCultureIgnoreCase) ||
                        fileName.Equals("SolosCrosshair.png", StringComparison.InvariantCultureIgnoreCase))
                    {
                        PreloadedList.Items.Add(item);
                    }
                    else
                    {
                        PersonalList.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading crosshairs: {ex.Message}");
                MessageBox.Show($"Error loading crosshairs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Enable Remove button only if a personal crosshair is selected.
        private void Crosshair_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RemoveButton.IsEnabled = PersonalList.SelectedItem is CrosshairItem;
        }

        private void AddCrosshair_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.png)|*.png",
                    Title = "Select a Crosshair Image"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var destinationPath = Path.Combine(crosshairDir, Path.GetFileName(openFileDialog.FileName));
                    File.Copy(openFileDialog.FileName, destinationPath, true);

                    // Added crosshairs are considered personal.
                    PersonalList.Items.Add(new CrosshairItem
                    {
                        FileName = Path.GetFileName(destinationPath),
                        FilePath = destinationPath
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding crosshair: {ex.Message}");
                MessageBox.Show($"Error adding crosshair: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveCrosshair_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PersonalList.SelectedItem is CrosshairItem personalItem)
                {
                    // If the overlay is using this crosshair image, clear it.
                    var overlay = Application.Current.Windows.OfType<CrosshairOverlay>().FirstOrDefault();
                    if (overlay != null && overlay.CrosshairImage.Source is BitmapImage currentImage)
                    {
                        if (currentImage.UriSource != null &&
                            string.Equals(currentImage.UriSource.LocalPath, personalItem.FilePath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            overlay.CrosshairImage.Source = null;
                        }
                    }

                    // Force garbage collection to release file handles.
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Delete the file.
                    if (File.Exists(personalItem.FilePath))
                    {
                        File.Delete(personalItem.FilePath);
                    }

                    // Reload the lists and disable the Remove button.
                    LoadCrosshairs();
                    RemoveButton.IsEnabled = false;
                }
                else if (PreloadedList.SelectedItem is CrosshairItem)
                {
                    MessageBox.Show("Preloaded crosshairs cannot be removed.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing crosshair: {ex.Message}");
                MessageBox.Show($"Error removing crosshair: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New method to rename a personal crosshair.
        private void RenameCrosshair_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PersonalList.SelectedItem is CrosshairItem personalItem)
                {
                    string currentName = personalItem.FileName;
                    string newName = Interaction.InputBox("Enter new name for the crosshair:", "Rename Crosshair", currentName);
                    if (!string.IsNullOrWhiteSpace(newName) && !newName.Equals(currentName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Ensure .png extension is preserved.
                        if (!newName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            newName += ".png";
                        }
                        string newPath = Path.Combine(crosshairDir, newName);

                        // Check if a file with the new name already exists.
                        if (File.Exists(newPath))
                        {
                            MessageBox.Show("A crosshair with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // If the overlay is using this image, clear it.
                        var overlay = Application.Current.Windows.OfType<CrosshairOverlay>().FirstOrDefault();
                        if (overlay != null && overlay.CrosshairImage.Source is BitmapImage currentImage)
                        {
                            if (currentImage.UriSource != null &&
                                string.Equals(currentImage.UriSource.LocalPath, personalItem.FilePath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                overlay.CrosshairImage.Source = null;
                            }
                        }

                        // Force garbage collection.
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // Rename (move) the file.
                        File.Move(personalItem.FilePath, newPath);

                        // Reload lists to reflect changes.
                        LoadCrosshairs();
                        MessageBox.Show("Crosshair renamed successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a personal crosshair to rename.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming crosshair: {ex.Message}");
                MessageBox.Show($"Error renaming crosshair: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Crosshair_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                CrosshairItem selectedItem = null;
                if (PreloadedList.SelectedItem is CrosshairItem preloadedItem)
                {
                    selectedItem = preloadedItem;
                }
                else if (PersonalList.SelectedItem is CrosshairItem personalItem)
                {
                    selectedItem = personalItem;
                }

                if (selectedItem != null)
                {
                    // Apply the crosshair: update settings and update overlay.
                    File.WriteAllText(settingsFile, selectedItem.FilePath);
                    var overlay = Application.Current.Windows.OfType<CrosshairOverlay>().FirstOrDefault();
                    if (overlay != null)
                    {
                        // Load image from stream with CacheOption.OnLoad to avoid locking the file.
                        BitmapImage bitmap = new BitmapImage();
                        using (var stream = new FileStream(selectedItem.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }
                        bitmap.Freeze();
                        overlay.CrosshairImage.Source = bitmap;
                    }
                    // No popup on double-click.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying crosshair: {ex.Message}");
                MessageBox.Show($"Error applying crosshair: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}