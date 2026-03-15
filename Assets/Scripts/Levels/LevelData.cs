using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LevelData")]
public class LevelData : ScriptableObject
{
    public TextAsset LevelGrid;
    public TextAsset ShooterGrid;
    public List<ConnectionConfig> ShooterConnectionConfigs;
    public int NumOfBlockLayers = 1;
    public int NumOfShooterCols = 3;
    public int NumOfDockCols = 2;
    public float ShooterCellGap = 1.5f;
    public int ShooterStartingAmmo = 60;
    public bool CanHideShooters = false;
}

[Serializable]
public struct ConnectionConfig
{
    public Vector2 Head;
    public Vector2 Tail;
}
