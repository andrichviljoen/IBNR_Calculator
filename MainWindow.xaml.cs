using System.Windows;
using IBNRCalculator.ViewModels;

namespace IBNRCalculator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (DataContext is null)
        {
            DataContext = new ReservingViewModel();
        }
    }
}
