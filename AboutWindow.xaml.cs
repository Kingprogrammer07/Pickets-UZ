using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Pickets;

public partial class AboutWindow : AccessibleDialogWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Versiya {DiagnosticsInfo.Version}";
    }

    private void CopyDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(DiagnosticsInfo.BuildSanitizedSummary());
            MessageBox.Show(this, "Shaxsiy ma’lumotlari yashirilgan diagnostika nusxalandi.", "Pickets",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Diagnostika ma’lumotlarini nusxalab bo‘lmadi.\n\n{ex.Message}", "Pickets",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var directory = Path.GetDirectoryName(LayoutStore.LayoutPath)!;
            Directory.CreateDirectory(directory);
            Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Pickets", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenGitHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://github.com/Kingprogrammer07/Pickets-UZ")
                { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Pickets", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
