using System.Windows;

namespace MCModVersionChecker.Views;

public partial class SettingsView : Window
{
    public SettingsView()
    {
        InitializeComponent();
        ApiKeyText.Text = Properties.Settings.Default.ApiKey;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.ApiKey = ApiKeyText.Text;
        Properties.Settings.Default.Save();
        Close();
    }
}