using System.Windows;
using BraveClipping.ViewModels;

namespace BraveClipping;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
