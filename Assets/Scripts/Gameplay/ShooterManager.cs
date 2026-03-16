using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ShooterManager : MonoBehaviour
{
    [SerializeField]
    private ShooterDock _shooterDock;
    [SerializeField]
    private ShooterGrid _shooterGrid;
    [SerializeField]
    private Shooter _shooterPrefab;
    [SerializeField]
    private int _shooterStartingAmmo = 2;
    [SerializeField]
    private ShooterConnection _shooterConnectionPrefab;
    private List<Shooter> _shooters = new();
    private List<ShooterConnection> _shooterConnections = new();

    [SerializeField]
    private Ammo _ammoPrefab;
    private IObjectPool<Ammo> _ammoPool;
     [SerializeField]
    private ParticleSystemVFX _hitVFXPrefab;
    [SerializeField]
    private AudioClip _startMergeSFX;
    [SerializeField]
    private AudioClip _endMergeSFX;
    private IObjectPool<ParticleSystemVFX> _hitVFXPool;

    private GameManager _gameManager;

    public void Setup(GameManager gameManager, LevelInfo levelInfo)
    {
        _gameManager = gameManager;
        _shooterStartingAmmo = levelInfo.ShooterStartingAmmo;
        SetupAmmoPool();
        SetupHitVfxPool();
        SetupDock(levelInfo.NumOfDockCols, levelInfo.ShooterCellGap);
        SetupGrid(levelInfo);
        SetupShooterConnections(levelInfo);
    }

    public Shooter AddShooter(GridNode gridNode, Color colour, int totalAmmo = -1, bool isUninteractable = false, bool isHideShooter = false)
    {
        var shooter = Instantiate(_shooterPrefab, _shooterGrid.transform);
        shooter.Setup(gridNode, colour, _ammoPool, _hitVFXPool, totalAmmo);
        shooter.OnClick += OnShooterClicked;
        shooter.OnAmmoConsumed += OnShooterAmmoConsumed;

        if (!isUninteractable)
        {
            _shooters.Add(shooter); // added to shooter grid
            TryHighlightShooter(shooter, shooter.GridNode);
        }
        else
        {
            shooter.IsMerged = true;
            _shooterDock.AddShooter(shooter, gridNode); // merged shooter added to dock
        }

        if (isHideShooter)
        {
            shooter.HideShooter();
        }

        shooter.IsUninteractable = isUninteractable;

        return shooter;
    }

    public async UniTask RemoveShooter(Shooter shooter)
    {
        _shooters.Remove(shooter);
        shooter.MoveToGridNode(null, false);
        await shooter.PlayDeathAnimation();
        Destroy(shooter.gameObject);
    }

    public Shooter GetShooter(Vector2 gridCoordinate)
    {
        return _shooters.FirstOrDefault(shooter => shooter.GridNode.GridCoordinate == gridCoordinate);
    }

    public void InitiateAttack(Shooter shooter)
    {
        shooter.InitiateAttack(_gameManager.GetShootableBlocks);
    }

    public void Clear()
    {
        for (int i = 0; i < _shooters.Count; i++)
        {
            Destroy(_shooters[i].gameObject);
        }

        _shooters.Clear();
        _shooterGrid.Clear();
        _shooterDock.Clear();
    }

    public void PlayStartMergeSFX()
    {
        AudioManager.Instance.PlayOneShot(_startMergeSFX);
    }

    public void PlayEndMergeSFX()
    {
        AudioManager.Instance.PlayOneShot(_endMergeSFX);
    }

    private void SetupAmmoPool()
    {
        _ammoPool = new ObjectPool<Ammo>(
            createFunc: () => 
                { 
                    var ammo = Instantiate(_ammoPrefab, transform); 
                    return ammo;
                },
            actionOnRelease: (ammo) => ammo.gameObject.SetActive(false),
            actionOnDestroy: (ammo) => Destroy(ammo.gameObject),
            defaultCapacity: 5,
            maxSize: 15
        );
        
        var initAmmos = new List<Ammo>();
        for (int i = 0; i < 5; i++)
        {
            initAmmos.Add(_ammoPool.Get());
        }

        for (int i = 0; i < 5; i++)
        {
            _ammoPool.Release(initAmmos[i]);
        }
    }

    private void SetupHitVfxPool()
    {
        _hitVFXPool = new ObjectPool<ParticleSystemVFX>(
            createFunc: () => 
                { 
                    var hitVfx = Instantiate(_hitVFXPrefab, transform); 
                    hitVfx.ObjectPool = _hitVFXPool;
                    return hitVfx;
                },
            actionOnGet: (hit) => hit.gameObject.SetActive(true),
            actionOnRelease: (hit) => hit.gameObject.SetActive(false),
            actionOnDestroy: (hit) => Destroy(hit.gameObject),
            maxSize: 15
        );
    }

    private void SetupDock(int numOfDockCols, float cellGap)
    {
        _shooterDock.Setup(numOfDockCols, cellGap);
    }

    private void SetupGrid(LevelInfo levelInfo)
    {
        var levelGrid = levelInfo.LevelGrid;
        var colourToCountMap = GetColourToShooterCountMap(levelGrid, levelInfo.NumOfBlockLayers);
        var totalShooters = levelGrid.Count * levelGrid[0].Length * levelInfo.NumOfBlockLayers / _shooterStartingAmmo;
        var rows = totalShooters / levelInfo.NumOfShooterCols;

        var colourKeys = colourToCountMap.Keys.ToList();
        _shooterGrid.Setup(new Vector2(rows, levelInfo.NumOfShooterCols), levelInfo.ShooterCellGap);
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < levelInfo.NumOfShooterCols; j++)
            {
                var gridNode = _shooterGrid.GetGridNode(new Vector2(i, j));
                Color colour;
                if (levelInfo.ShooterGrid.Count != 0)
                {
                    colour = ColourUtils.GetColorFromHex(levelInfo.ShooterGrid[i][j]);
                }
                else
                {
                    var randomKey = colourKeys[Random.Range(0, colourKeys.Count - 1)];
                    colour = ColourUtils.GetColorFromHex(randomKey);
                    colourToCountMap[randomKey]--;
                    if (colourToCountMap[randomKey] == 0)
                    {
                        colourKeys.Remove(randomKey);
                    }
                }

                var isHideShooter = levelInfo.CanHideShooters && gridNode.GridCoordinate.x != 0;
                AddShooter(gridNode, colour, _shooterStartingAmmo, isHideShooter: isHideShooter);
            }
        }
    }

    private void SetupShooterConnections(LevelInfo levelInfo)
    {
        foreach (KeyValuePair<Vector2, Vector2> kvp in levelInfo.ShooterConnectionMap)
        {
            var headShooter = GetShooter(kvp.Key);
            var tailShooter = GetShooter(kvp.Value);
            CreateConnection(headShooter, tailShooter);
        }
    }

    private void CreateConnection(Shooter headShooter, Shooter tailShooter)
    {
        var connection = Instantiate(_shooterConnectionPrefab, transform);
        connection.Setup(headShooter, tailShooter);
        _shooterConnections.Add(connection);
    }

    private Dictionary<string, int> GetColourToShooterCountMap(List<string[]> levelGrid, int layers)
    {
        var tempMap = new Dictionary<string, int>();
        var mapToReturn = new Dictionary<string, int>();
        for (int i = 0; i < levelGrid.Count; i++)
        {
            var row = levelGrid[i];
            for (int j = 0; j < row.Length; j++)
            {
                var col = row[j];
                if (col == "#000000" || col == "#FFFFFF")
                    continue;

                if (tempMap.ContainsKey(col))
                {
                    tempMap[col] += layers;
                }
                else
                {
                    tempMap.Add(col, layers);
                }
            }
        }

        foreach (KeyValuePair<string, int> item in tempMap)
        {
            mapToReturn.Add(item.Key, item.Value / _shooterStartingAmmo);
        }

        return mapToReturn;
    }

    private async void OnShooterClicked(Shooter shooter)
    {
        // If has connection
        if (shooter.HasConnection)
        {
            var connection = shooter.Connection;
            List<GridNode> emptyDockGridNodes;
            var canDock = _shooterDock.TryGetAdjacentEmptyGridNodes(out emptyDockGridNodes);
            if (canDock)
            {
                await HandleDockingAndAttacking(connection.Head, emptyDockGridNodes[0]);
                HandleDockingAndAttacking(connection.Tail, emptyDockGridNodes[1]);
            }
        }
        else
        {
            GridNode emptyDockGridNode;
            var canDock = _shooterDock.TryGetEmptyGridNode(out emptyDockGridNode);
            if (canDock)
            {
                HandleDockingAndAttacking(shooter, emptyDockGridNode);
            }
        }
    }

    private async UniTask HandleDockingAndAttacking(Shooter shooter, GridNode emptyDockGridNode)
    {
        await DockShooterAndUpdateGrid(shooter, emptyDockGridNode);
        TryMergeShootersAndAttack(shooter);
    }

    private async UniTask DockShooterAndUpdateGrid(Shooter shooter, GridNode emptyDockGridNode)
    {
        shooter.SetIsClickable(false);
        var affectedColumn = shooter.GridNode.GridCoordinate.y;
        UpdateShooterGrid(shooter, affectedColumn);
        
        await shooter.MoveToGridNode(emptyDockGridNode);

        _shooterDock.AddShooter(shooter, emptyDockGridNode);
    }

    private async UniTask TryMergeShootersAndAttack(Shooter shooter)
    {
        var mergedShooter = await ShooterMerger.TryMergeShooters(shooter.Colour, _shooterDock, this);

        if (mergedShooter != null)
        {
            mergedShooter.InitiateAttack(_gameManager.GetShootableBlocks);
        }
        else
        {
            shooter.InitiateAttack(_gameManager.GetShootableBlocks);
        }
    }

    private void OnShooterAmmoConsumed(Shooter shooter)
    {
        if (shooter.HasConnection)
        {
            HandleConnectedShooterDeath(shooter);
            return;
        }
        _shooterDock.RemoveShooter(shooter);
        RemoveShooter(shooter);
    }

    private void HandleConnectedShooterDeath(Shooter shooter)
    {
        if (shooter.Connection == null)
            return;

        var otherShooter = (shooter == shooter.Connection.Head)? shooter.Connection.Tail : shooter.Connection.Head;

        if (!otherShooter.IsAlive)
        {
            _shooterConnections.Remove(shooter.Connection);
            Destroy(shooter.Connection.gameObject);

            _shooterDock.RemoveShooter(shooter);
            _shooterDock.RemoveShooter(otherShooter);
            RemoveShooter(shooter);
            RemoveShooter(otherShooter);
        }
    }

    private async void UpdateShooterGrid(Shooter dockedShooter, float affectedColumn)
    {
        _shooters.Remove(dockedShooter);
        //var affectedRow = dockedShooter.GridNode.GridCoordinate.x;

        for (int i = 0; i < _shooters.Count; i++)
        {
            var shooter = _shooters[i];
            var oldGridNode = shooter.GridNode;
            if (oldGridNode.GridCoordinate.y == affectedColumn)
            {
                var newGridNode = _shooterGrid.GetGridNode(new Vector2(oldGridNode.GridCoordinate.x - 1, oldGridNode.GridCoordinate.y));
                shooter.MoveToGridNode(newGridNode);

                TryHighlightShooter(shooter, newGridNode);
            }
        }
    }

    private void TryHighlightShooter(Shooter shooter, GridNode gridNode)
    {
        if (gridNode.GridCoordinate.x != 0)
            return;

        shooter.SetIsClickable(true);
        shooter.RevealShooter();
    }
}
