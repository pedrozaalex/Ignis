using System.Text.Json;

namespace Samples.Breakout.Services;

/// <summary>
/// Represents a single leaderboard entry.
/// </summary>
public record LeaderboardEntry(string Initials, int Score, int Level, DateTime Date);

/// <summary>
/// Manages high scores with optional file persistence.
/// </summary>
public sealed class LeaderboardService
{
    private const int MaxEntries = 10;
    private const string FileName = "leaderboard.json";
    
    private List<LeaderboardEntry> _entries = [];
    
    public IReadOnlyList<LeaderboardEntry> Entries => _entries;
    
    public bool IsHighScore(int score) => 
        _entries.Count < MaxEntries || score > _entries[^1].Score;
    
    public void AddEntry(string initials, int score, int level)
    {
        var entry = new LeaderboardEntry(
            initials.ToUpperInvariant().PadRight(3)[..3],
            score,
            level,
            DateTime.Now
        );
        
        _entries.Add(entry);
        _entries = _entries
            .OrderByDescending(e => e.Score)
            .Take(MaxEntries)
            .ToList();
    }
    
    public void Load()
    {
        try
        {
            var path = GetSavePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var entries = JsonSerializer.Deserialize<List<LeaderboardEntry>>(json);
                if (entries != null)
                    _entries = entries;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load leaderboard: {ex.Message}");
            _entries = [];
        }
    }
    
    public void Save()
    {
        try
        {
            var path = GetSavePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            
            var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save leaderboard: {ex.Message}");
        }
    }
    
    private static string GetSavePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Ignis", "Breakout", FileName);
    }
}
