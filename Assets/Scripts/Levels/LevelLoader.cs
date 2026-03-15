using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    private const string _pathPrefix = "Levels/";

    public LevelInfo LoadLevel(int level)
    {
        var path = _pathPrefix + $"Level{level}Data";
        var levelData = Resources.Load<LevelData>(path);

        if (levelData == null)
        {
            Debug.LogError("Level Data not found in Resources/Levels/");
            return null;
        }

        // Block Grid
        List<string[]> levelGrid = new();

        string[] lines = levelData.LevelGrid.text.Split("\n");

        for (int i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            string[] values = line.Trim().Split(","); // column values
            values = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
            if (values.Length == 0)
            {
                continue;
            }
            levelGrid.Add(values); // add line as a row
        }

        // Shooter Grid
        List<string[]> shooterGrid = new();
        if (levelData.ShooterGrid != null)
        {

            string[] shooterLines = levelData.ShooterGrid.text.Split("\n");

            for (int i = 0; i < shooterLines.Length; i++)
            {
                var line = shooterLines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] values = line.Trim().Split(","); // column values
                values = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                if (values.Length == 0)
                {
                    continue;
                }
                shooterGrid.Add(values); // add line as a row
            }
        }

        // Shooter Connections
        var connectionMap = new Dictionary<Vector2, Vector2>();
        for (int i = 0; i < levelData.ShooterConnectionConfigs.Count; i++)
        {
            var config = levelData.ShooterConnectionConfigs[i];
            connectionMap.Add(config.Head, config.Tail);
        }

        return new LevelInfo{ LevelGrid = levelGrid, ShooterGrid = shooterGrid, ShooterConnectionMap = connectionMap , NumOfBlockLayers = levelData.NumOfBlockLayers, NumOfShooterCols = levelData.NumOfShooterCols, NumOfDockCols = levelData.NumOfDockCols, ShooterCellGap = levelData.ShooterCellGap, ShooterStartingAmmo = levelData.ShooterStartingAmmo, CanHideShooters = levelData.CanHideShooters };
    }
}
