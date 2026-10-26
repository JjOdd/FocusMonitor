using Avalonia;

namespace FocusMonitor
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Contains("--selftest")) return SelfTest.Run();
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }
}