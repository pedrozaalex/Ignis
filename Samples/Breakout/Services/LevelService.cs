using System.Numerics;
using System.Text.Json;
using Samples.Breakout.Core;

namespace Samples.Breakout.Services;

/// <summary>
/// Level data loaded from JSON.
/// </summary>
public class LevelData
{
    public int LevelNumber { get; set; }
    public string Name { get; set; } = "";
    public float BallSpeedMultiplier { get; set; } = 1.0f;
    public int[][] BrickGrid { get; set; } = [];
}

/// <summary>
/// Manages level loading and progression.
/// </summary>
public sealed class LevelService
{
    private readonly List<LevelData> _levels = [];
    
    public int LevelCount => _levels.Count;
    
    public LevelData? GetLevel(int levelNumber)
    {
        var index = levelNumber - 1;
        return index >= 0 && index < _levels.Count ? _levels[index] : null;
    }
    
    public void LoadLevels()
    {
        var levelsPath = Path.Combine(AppContext.BaseDirectory, "Levels");
        
        if (Directory.Exists(levelsPath))
        {
            foreach (var file in Directory.GetFiles(levelsPath, "*.json").OrderBy(f => f))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var level = JsonSerializer.Deserialize<LevelData>(json);
                    if (level != null)
                        _levels.Add(level);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load level {file}: {ex.Message}");
                }
            }
        }
        
        // If no levels loaded, create default levels
        if (_levels.Count == 0)
        {
            _levels.AddRange(CreateDefaultLevels());
        }
    }
    
    public List<Brick> CreateBricksForLevel(int levelNumber, float screenWidth, float topMargin)
    {
        var level = GetLevel(levelNumber) ?? CreateDefaultLevels()[0];
        return CreateBricksFromGrid(level.BrickGrid, screenWidth, topMargin);
    }
    
    private List<Brick> CreateBricksFromGrid(int[][] grid, float screenWidth, float topMargin)
    {
        var bricks = new List<Brick>();
        
        const float brickWidth = 50f;
        const float brickHeight = 20f;
        const float padding = 4f;
        
        var rows = grid.Length;
        var cols = rows > 0 ? grid[0].Length : 0;
        
        var totalWidth = cols * (brickWidth + padding) - padding;
        var startX = (screenWidth - totalWidth) / 2;
        
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < grid[row].Length; col++)
            {
                var brickType = grid[row][col];
                if (brickType == 0) continue; // Empty space
                
                var position = new Vector2(
                    startX + col * (brickWidth + padding),
                    topMargin + row * (brickHeight + padding)
                );
                
                var type = brickType switch
                {
                    2 => BrickType.Hard,
                    3 => BrickType.Unbreakable,
                    4 => BrickType.PowerUp,
                    _ => BrickType.Normal
                };
                
                var brick = new Brick(position, type, row % 6)
                {
                    Size = new Vector2(brickWidth, brickHeight)
                };
                bricks.Add(brick);
            }
        }
        
        return bricks;
    }
    
    private List<LevelData> CreateDefaultLevels()
    {
        return
        [
            new LevelData
            {
                LevelNumber = 1,
                Name = "Getting Started",
                BallSpeedMultiplier = 1.0f,
                BrickGrid =
                [
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
                    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1]
                ]
            },
            new LevelData
            {
                LevelNumber = 2,
                Name = "Tough Rows",
                BallSpeedMultiplier = 1.1f,
                BrickGrid =
                [
                    [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
                    [1, 1, 1, 4, 1, 1, 1, 1, 4, 1, 1, 1],
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
                    [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2]
                ]
            },
            new LevelData
            {
                LevelNumber = 3,
                Name = "The Wall",
                BallSpeedMultiplier = 1.2f,
                BrickGrid =
                [
                    [3, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 3],
                    [3, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 3],
                    [3, 0, 1, 4, 1, 1, 1, 1, 4, 1, 0, 3],
                    [3, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 3],
                    [3, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 3],
                    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1]
                ]
            },
            new LevelData
            {
                LevelNumber = 4,
                Name = "Diamond",
                BallSpeedMultiplier = 1.3f,
                BrickGrid =
                [
                    [0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0],
                    [0, 0, 0, 0, 2, 1, 1, 2, 0, 0, 0, 0],
                    [0, 0, 0, 2, 1, 4, 4, 1, 2, 0, 0, 0],
                    [0, 0, 2, 1, 1, 1, 1, 1, 1, 2, 0, 0],
                    [0, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0],
                    [0, 0, 2, 1, 1, 1, 1, 1, 1, 2, 0, 0],
                    [0, 0, 0, 2, 1, 1, 1, 1, 2, 0, 0, 0],
                    [0, 0, 0, 0, 2, 1, 1, 2, 0, 0, 0, 0],
                    [0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0]
                ]
            },
            new LevelData
            {
                LevelNumber = 5,
                Name = "Final Challenge",
                BallSpeedMultiplier = 1.5f,
                BrickGrid =
                [
                    [3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3],
                    [3, 2, 4, 1, 1, 1, 1, 1, 1, 4, 2, 3],
                    [3, 2, 1, 2, 2, 2, 2, 2, 2, 1, 2, 3],
                    [3, 2, 1, 2, 4, 1, 1, 4, 2, 1, 2, 3],
                    [3, 2, 1, 2, 1, 2, 2, 1, 2, 1, 2, 3],
                    [3, 2, 1, 2, 1, 2, 2, 1, 2, 1, 2, 3],
                    [3, 2, 1, 2, 4, 1, 1, 4, 2, 1, 2, 3],
                    [3, 2, 1, 2, 2, 2, 2, 2, 2, 1, 2, 3],
                    [3, 2, 4, 1, 1, 1, 1, 1, 1, 4, 2, 3],
                    [3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3]
                ]
            }
        ];
    }
}
