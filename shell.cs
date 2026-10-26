using System.Diagnostics;

namespace FocusMonitor
{
    internal static class Shell
    {
        public static string? Run(string file, params string[] args)
        {
            try
            {
                var psi = new ProcessStartInfo(file)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                foreach (var a in args) psi.ArgumentList.Add(a);

                using var p = Process.Start(psi);
                if (p == null) return null;
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(2000);
                return p.ExitCode == 0 ? output : null;
            }
            catch { return null; }
        }
    }
}