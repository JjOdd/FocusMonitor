namespace FocusMonitor
{
    internal interface IPlatformService
    {
        (string? ProcessName, string? Title) GetActiveWindowInfo();
        double GetIdleSeconds();
        void OpenPath(string path);
    }

    internal static class PlatformFactory
    {
        public static IPlatformService Create()
        {
            if (OperatingSystem.IsWindows()) return new WindowsPlatform();
            if (OperatingSystem.IsMacOS()) return new MacPlatform();
            if (OperatingSystem.IsLinux()) return new LinuxPlatform();
            throw new PlatformNotSupportedException();
        }
    }
}