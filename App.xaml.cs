using System.Linq;
using System.Windows;
using IBNRCalculator.ViewModels;

namespace IBNRCalculator;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var runConsole = e.Args.Contains("--console");
        var filteredArgs = e.Args.Where(arg => arg != "--console").ToArray();

        if (runConsole)
        {
            var cli = new CliRunner();
            await cli.RunAsync(filteredArgs);
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow();
        if (mainWindow.DataContext is null)
        {
            mainWindow.DataContext = new ReservingViewModel();
        }

        mainWindow.Show();
    }
}
