using MCModVersionChecker.ViewModels;
using System.Windows;

namespace MCModVersionChecker;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}