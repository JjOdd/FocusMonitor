using System.Text.Json.Serialization;

namespace FocusMonitor.Models
{
    public class AppUsage
    {
        [JsonPropertyName("TotalSeconds")]
        public int TotalSeconds { get; set; }

        [JsonPropertyName("titles")]
        public Dictionary<string, int> Titles { get; set; } = new();

        [JsonPropertyName("category")]
        public string Category { get; set; } = "Neutral";
    }

    public class DistractionEvent
    {
        [JsonPropertyName("app")]
        public string App { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("time")]
        public string Time { get; set; } = "";

        [JsonPropertyName("duration_seconds")]
        public int DurationSeconds { get; set; }
    }

    public class DayData
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("apps")]
        public Dictionary<string, AppUsage> Apps { get; set; } = new();

        [JsonPropertyName("idle_seconds")]
        public int IdleSeconds { get; set; }

        [JsonPropertyName("events")]
        public List<DistractionEvent> Events { get; set; } = new();
    }

    public class Categories
    {
        [JsonPropertyName("Distracting")]
        public List<string> Distracting { get; set; } = new();

        [JsonPropertyName("Productive")]
        public List<string> Productive { get; set; } = new();


// add pa ng mga distraction at proiductivity. tandaan mo
        public static Categories Default() => new()
        {
            Distracting = new List<string>
            {
                "youtube", "facebook", "instagram", "tiktok", "twitter", "x.com", "Shopee", "Lazada",
                "reddit", "netflix", "twitch", "discord", "steam", "spotify"
            },
            Productive = new List<string>
            {
                "code", "visual studio", "pycharm", "terminal",
                "cmd.exe", "powershell", "windows terminal", "word", "excel",
                "powerpoint", "outlook", "notion", "slack", "teams", "notepad++", "chrome - github",
                "stack overflow"
            }
        };
    }

    public class Snapshot
    {
        public string? CurrentProc { get; set; }
        public string? CurrentTitle { get; set; }
        public string CurrentCategory { get; set; } = "Neutral";
        public double SessionSeconds { get; set; }
        public DateTime SessionStart { get; set; }
        public Dictionary<string, AppUsage> Apps { get; set; } = new();
        public int IdleSeconds { get; set; }
        public List<DistractionEvent> Events { get; set; } = new();
    }
}