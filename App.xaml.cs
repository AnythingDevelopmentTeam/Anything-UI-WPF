using System.Windows;
using Wpf.Ui.Appearance;

namespace Anything_UI_WPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            System.Windows.MessageBox.Show(
                $"Unhandled error:\n\n{args.Exception}",
                "Fatal Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            System.Windows.MessageBox.Show(
                $"Unhandled domain error:\n\n{args.ExceptionObject}",
                "Fatal Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        };

        ApplicationThemeManager.Apply(ApplicationTheme.Light);

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
