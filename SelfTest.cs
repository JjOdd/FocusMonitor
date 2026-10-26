using System.Runtime.InteropServices;

namespace FocusMonitor
{
    internal static class SelfTest
    {
        public static int Run()
        {
            var platform = PlatformFactory.Create();
            var (proc, title) = platform.GetActiveWindowInfo();
            double idle = platform.GetIdleSeconds();

            Console.WriteLine($"OS:       {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Platform: {platform.GetType().Name}");
            Console.WriteLine($"Process:  {proc ?? "(null)"}");
            Console.WriteLine($"Title:    {title ?? "(null)"}");
            Console.WriteLine($"Idle sec: {idle:F2}");

            return proc != null ? 0 : 1;
        }
    }
}
