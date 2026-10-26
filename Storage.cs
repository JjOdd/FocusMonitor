using System.Text.Json;
using FocusMonitor.Models;

namespace FocusMonitor
{
    internal static class Storage
    {
        public static readonly string DataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".focus_tracker");

        public static readonly string CategoriesFile = Path.Combine(DataDir, "categories.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        static Storage()
        {
            Directory.CreateDirectory(DataDir);
        }

        public static Categories LoadCategories()
        {
            if (!File.Exists(CategoriesFile))
            {
                var defaults = Categories.Default();
                File.WriteAllText(CategoriesFile, JsonSerializer.Serialize(defaults, JsonOptions));
                return defaults;
            }

            try
            {
                string json = File.ReadAllText(CategoriesFile);
                return JsonSerializer.Deserialize<Categories>(json) ?? Categories.Default();
            }
            catch
            {
                return Categories.Default();
            }
        }

        public static string DataFileFor(DateOnly day) =>
            Path.Combine(DataDir, $"{day:yyyy-MM-dd}.json");

        public static DayData LoadDataFor(DateOnly day)
        {
            string path = DataFileFor(day);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var loaded = JsonSerializer.Deserialize<DayData>(json);
                    if (loaded != null)
                    {
                        loaded.Apps ??= new();
                        loaded.Events ??= new();
                        return loaded;
                    }
                }
                catch
                {

                }
            }

            return new DayData { Date = day.ToString("yyyy-MM-dd"), Apps = new(), IdleSeconds = 0, Events = new() };
        }

        public static void SaveData(DayData data)
        {
            var day = DateOnly.Parse(data.Date);
            string path = DataFileFor(day);
            string tmpPath = path + "SaveData";
            File.WriteAllText(tmpPath, JsonSerializer.Serialize(data, JsonOptions));
            File.Move(tmpPath, path, overwrite: true);
        }
    }
}