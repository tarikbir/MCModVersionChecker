using MCModVersionChecker.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace MCModVersionChecker;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        TryOpenSelectedMod();
    }

    private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TryOpenSelectedMod();
        }
    }

    private void TryOpenSelectedMod()
    {
        if (DataContext is MainViewModel vm && vm.OpenModLinkCommand.CanExecute(null))
        {
            vm.OpenModLinkCommand.Execute(null);
        }
    }
}