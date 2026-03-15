using System.Collections.Generic;
using UnityEngine;

public class LevelInfo
{
    public List<string[]> LevelGrid;
    public List<string[]> ShooterGrid;
    public Dictionary<Vector2, Vector2> ShooterConnectionMap;
    public int NumOfBlockLayers;
    public int NumOfShooterCols;
    public int NumOfDockCols;
    public float ShooterCellGap;
    public int ShooterStartingAmmo;
    public bool CanHideShooters;
}
