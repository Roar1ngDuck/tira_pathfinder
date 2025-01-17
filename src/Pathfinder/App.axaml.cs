using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Pathfinder;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            var args = desktop.Args;
            if (args.Length == 3)
            {
                var mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.MapTextBox.Text = args[0];
                mainWindow.StartTextBox.Text = args[1];
                mainWindow.GoalTextBox.Text = args[2];
                mainWindow.StartVisualization();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}