using System.Runtime.Versioning;

namespace FocusMonitor
{
    [SupportedOSPlatform("linux")]
    internal class LinuxPlatform : IPlatformService
    {
        public (string? ProcessName, string? Title) GetActiveWindowInfo()
        {
            if (Environment.GetEnvironmentVariable("XDG_SESSION_TYPE") == "wayland")
                return (null, null);

            string? title = Shell.Run("xdotool", "getactivewindow", "getwindowname")?.Trim();
            string? pid = Shell.Run("xdotool", "getactivewindow", "getwindowpid")?.Trim();
            if (title == null || pid == null) return (null, null);

            string? proc = Shell.Run("ps", "-p", pid, "-o", "comm=")?.Trim();
            return (proc ?? "Unknown", title.Length > 0 ? title : "(untitled)");
        }

        public double GetIdleSeconds()
        {
            string? ms = Shell.Run("xprintidle")?.Trim();
            return double.TryParse(ms, out var v) ? v / 1000.0 : 0;
        }

        public void OpenPath(string path) => Shell.Run("xdg-open", path);
    }
}