using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FocusMonitor.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace FocusMonitor
{
    internal partial class MainWindow : Window
    {
        internal record AppRow(string App, string Category, string Time);
        internal record TimelineRow(string When, string App, string Window, string Duration);

        private readonly Tracker _tracker;
        private readonly IPlatformService _platform;
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        private static readonly Dictionary<string, string> CategoryColors = new()
        {
            ["Productive"] = "#2e7d32",
            ["Distracting"] = "#c62828",
            ["Neutral"] = "#616161",
            ["Idle"] = "#9e9e9e"
        };

        public MainWindow(Tracker tracker, IPlatformService platform)
        {
            _tracker = tracker;
            _platform = platform;
            InitializeComponent();
            _timer.Tick += (_, _) => RefreshUI();
            _timer.Start();
            RefreshUI();
        }

        private static string FormatDuration(double seconds)
        {
            int total = (int)seconds;
            int h = total / 3600;
            int m = (total % 3600) / 60;
            int s = total % 60;
            if (h > 0) return $"{h}h {m}m {s}s";
            if (m > 0) return $"{m}m {s}s";
            return $"{s}s";
        }

        private void RefreshUI()
        {
            Snapshot snap = _tracker.GetSnapshot();

            AppLabel.Text = snap.CurrentProc ?? "—";
            TitleLabel.Text = snap.CurrentTitle ?? "";
            CategoryLabel.Text = snap.CurrentCategory;
            CategoryLabel.Foreground = new SolidColorBrush(Color.Parse(
                CategoryColors.GetValueOrDefault(snap.CurrentCategory, "#ffffff")));
            SessionLabel.Text = $"session: {FormatDuration(snap.SessionSeconds)}";

            var totals = new Dictionary<string, double>
            {
                ["Productive"] = 0,
                ["Distracting"] = 0,
                ["Neutral"] = 0,
                ["Idle"] = snap.IdleSeconds
            };
            foreach (var info in snap.Apps.Values)
                totals[info.Category] = totals.GetValueOrDefault(info.Category, 0) + info.TotalSeconds;

            ProductiveValue.Text = FormatDuration(totals["Productive"]);
            DistractingValue.Text = FormatDuration(totals["Distracting"]);
            NeutralValue.Text = FormatDuration(totals["Neutral"]);
            IdleValue.Text = FormatDuration(totals["Idle"]);

            AppsGrid.ItemsSource = snap.Apps
                .OrderByDescending(kv => kv.Value.TotalSeconds)
                .Take(15)
                .Select(kv => new AppRow(kv.Key, kv.Value.Category, FormatDuration(kv.Value.TotalSeconds)))
                .ToList();

            var rows = new List<TimelineRow>();
            if (snap.CurrentCategory == "Distracting" && snap.SessionSeconds >= 3)
            {
                rows.Add(new TimelineRow(
                    snap.SessionStart.ToString("h:mm tt"),
                    snap.CurrentProc ?? "",
                    snap.CurrentTitle ?? "",
                    FormatDuration(snap.SessionSeconds) + " (ongoing)"));
            }
            rows.AddRange(snap.Events.Select(e =>
                new TimelineRow(e.Time, e.App, e.Title, FormatDuration(e.DurationSeconds))));
            TimelineGrid.ItemsSource = rows;
        }

        private void EditCategories_Click(object? sender, RoutedEventArgs e) =>
            _platform.OpenPath(Storage.CategoriesFile);

        private void OpenDataFolder_Click(object? sender, RoutedEventArgs e) =>
            _platform.OpenPath(Storage.DataDir);

        private async void ResetToday_Click(object? sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Reset Today", "Clear all tracked data for today?", ButtonEnum.YesNo);
            if (await box.ShowAsync() == ButtonResult.Yes)
                _tracker.ResetToday();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            _tracker.Stop();
            base.OnClosing(e);
        }
    }
}