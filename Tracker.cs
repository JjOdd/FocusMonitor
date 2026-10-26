using FocusMonitor.Models;

namespace FocusMonitor
{
    internal class Tracker
    {
        private const int IdleThresholdSeconds = 90;
        private const int SaveIntervalSeconds = 15;
        private const int PollIntervalSeconds = 1;
        private const int MinDistractionEventSeconds = 5;
        private const int MaxEventsStored = 300;

        private readonly object _lock = new();
        private readonly IPlatformService _platform;
        private Categories _categories;
        private DateOnly _today;
        private DayData _data;

        private string? _currentProc;
        private string? _currentTitle;
        private string _currentCategory = "Neutral";
        private DateTime _sessionStart;
        private bool _isIdle;

        private volatile bool _running;
        private Thread? _thread;

  public Tracker(IPlatformService platform)
{
    _platform = platform;
    _categories = Storage.LoadCategories();
    _today = DateOnly.FromDateTime(DateTime.Now);
    _data = Storage.LoadDataFor(_today);
    _sessionStart = DateTime.Now;
}
        public void Start()
        {
            _running = true;
            _thread = new Thread(Loop) { IsBackground = true };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            lock (_lock)
            {
                Storage.SaveData(_data);
            }
        }

        public void ResetToday()
        {
            lock (_lock)
            {
                _data = new DayData { Date = _today.ToString("yyyy-MM-dd"), Apps = new(), IdleSeconds = 0, Events = new() };
                Storage.SaveData(_data);
                _sessionStart = DateTime.Now;
            }
        }
        private static string Categorize(string proc, string title, Categories categories)
        {
            string text = $"{proc} {title}".ToLowerInvariant();

            foreach (var kw in categories.Distracting)
                if (text.Contains(kw.ToLowerInvariant())) return "Distracting";

            foreach (var kw in categories.Productive)
                if (text.Contains(kw.ToLowerInvariant())) return "Productive";

            return "Neutral";
        }

        private void AddDistractionEvent(string proc, string? title, DateTime startTs, double duration)
        {
            _data.Events.Add(new DistractionEvent
            {
                App = proc,
                Title = (title ?? "").Length > 120 ? title!.Substring(0, 120) : (title ?? ""),
                Time = startTs.ToString("h:mm tt"),
                DurationSeconds = (int)duration
            });

            if (_data.Events.Count > MaxEventsStored)
                _data.Events.RemoveRange(0, _data.Events.Count - MaxEventsStored);
        }

        private void ICheckNewDay()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (today != _today)
            {
                Storage.SaveData(_data);
                _today = today;
                _data = Storage.LoadDataFor(_today);
            }
        }

        private void Loop()
        {
            DateTime lastSave = DateTime.Now;

            while (_running)
            {
                ICheckNewDay();

              double idle = _platform.GetIdleSeconds();
                bool idleNow = idle >= IdleThresholdSeconds;

                var (proc, title) = _platform.GetActiveWindowInfo();
                DateTime now = DateTime.Now;

                lock (_lock)
                {
                    bool changed = proc != _currentProc || title != _currentTitle || idleNow != _isIdle;
                    if (changed)
                    {
                        double endedDuration = (now - _sessionStart).TotalSeconds;
                        if (_currentCategory == "Distracting" && _currentProc != null
                            && endedDuration >= MinDistractionEventSeconds)
                        {
                            AddDistractionEvent(_currentProc, _currentTitle, _sessionStart, endedDuration);
                        }

                        _currentProc = proc;
                        _currentTitle = title;
                        _isIdle = idleNow;
                        _sessionStart = now;
                        _currentCategory = idleNow ? "Idle" : Categorize(proc ?? "", title ?? "", _categories);
                    }

                    if (idleNow)
                    {
                        _data.IdleSeconds += PollIntervalSeconds;
                    }
                    else if (proc != null)
                    {
                        string cat = Categorize(proc, title ?? "", _categories);
                        _currentCategory = cat;

                        if (!_data.Apps.TryGetValue(proc, out var entry)) 
                        {
                            entry = new AppUsage { TotalSeconds = 0, Titles = new(), Category = cat };
                            _data.Apps[proc] = entry;
                        }
                        entry.TotalSeconds += PollIntervalSeconds;
                        entry.Category = cat;

                        string key = (title ?? "(untitled)");
                        if (key.Length > 120) key = key.Substring(0, 120);
                        entry.Titles[key] = entry.Titles.GetValueOrDefault(key, 0) + PollIntervalSeconds;
                    }
                }

                if ((now - lastSave).TotalSeconds >= SaveIntervalSeconds)
                {
                    lock (_lock)
                    {
                        Storage.SaveData(_data);
                    }
                    lastSave = now;
                }

                Thread.Sleep(PollIntervalSeconds * 1000);
            }
        }

        public Snapshot GetSnapshot()
        {
            lock (_lock)
            {
                var appsCopy = _data.Apps.ToDictionary(
                    kv => kv.Key,
                    kv => new AppUsage { TotalSeconds = kv.Value.TotalSeconds, Category = kv.Value.Category });

                var eventsCopy = _data.Events.TakeLast(30).Reverse().ToList();

                return new Snapshot
                {
                    CurrentProc = _currentProc,
                    CurrentTitle = _currentTitle,
                    CurrentCategory = _currentCategory,
                    SessionSeconds = (DateTime.Now - _sessionStart).TotalSeconds,
                    SessionStart = _sessionStart,
                    Apps = appsCopy,
                    IdleSeconds = _data.IdleSeconds,
                    Events = eventsCopy
                };
            }
        }
    }
}