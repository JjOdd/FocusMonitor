using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace FocusMonitor
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var platform = PlatformFactory.Create();
                var tracker = new Tracker(platform);
                tracker.Start();
                desktop.MainWindow = new MainWindow(tracker, platform);
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}