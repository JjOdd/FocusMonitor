using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FocusMonitor
{
    internal class WindowsPlatform : IPlatformService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public (string? ProcessName, string? Title) GetActiveWindowInfo()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return (null, null);

            var sb = new StringBuilder(512);
            GetWindowText(hwnd, sb, sb.Capacity);
            string title = sb.Length > 0 ? sb.ToString() : "(untitled)";

            string processName = "Unknown";
            try
            {
                GetWindowThreadProcessId(hwnd, out uint pid);
                using var proc = Process.GetProcessById((int)pid);
                processName = proc.ProcessName + ".exe";
            }
            catch
            {
                
            }

            return (processName, title);
        }

               public double GetIdleSeconds()
        {
            var lii = new LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(lii);
            if (!GetLastInputInfo(ref lii))
                return 0;

            uint idleTicks = GetTickCount() - lii.dwTime;
            return idleTicks / 1000.0;
        }

        public void OpenPath(string path) =>
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }       // <- dulo ng class
}           // <- dulo ng namespace