using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FocusMonitor
{
    [SupportedOSPlatform("macos")]
    internal class MacPlatform : IPlatformService
    {
        [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
        private static extern double CGEventSourceSecondsSinceLastEventType(int stateID, uint eventType);

        public double GetIdleSeconds() =>
            CGEventSourceSecondsSinceLastEventType(1, 0xFFFFFFFF);

        public (string? ProcessName, string? Title) GetActiveWindowInfo()
        {
            const string script = @"
                tell application ""System Events""
                    set p to first application process whose frontmost is true
                    set n to name of p
                    try
                        set t to name of front window of p
                    on error
                        set t to """"
                    end try
                    return n & ""|"" & t
                end tell";

            string? output = Shell.Run("osascript", "-e", script)?.Trim();
            if (string.IsNullOrEmpty(output)) return (null, null);

            var parts = output.Split('|', 2);
            string title = parts.Length > 1 && parts[1].Length > 0 ? parts[1] : "(untitled)";
            return (parts[0], title);
        }

        public void OpenPath(string path) => Shell.Run("open", path);
    }
}