using System.Windows;

namespace MCModVersionChecker.Views;

public partial class SettingsView : Window
{
    public SettingsView()
    {
        InitializeComponent();
        ApiKeyText.Text = Properties.Settings.Default.ApiKey;
        ApiKeyText.Focus();
        ApiKeyText.CaretIndex = ApiKeyText.Text.Length;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.ApiKey = ApiKeyText.Text.Trim();
        Properties.Settings.Default.Save();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
