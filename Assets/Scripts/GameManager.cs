using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int CurrentLevel { get; private set; }
    public float CurrentLevelProgress { get; private set; }
    public event Action OnLevelStarted;
    public event Action OnLevelProgressUpdated;
    public event Action<int> OnLevelEnded;

    [SerializeField]
    private LevelLoader _levelLoader;
    [SerializeField]
    private BlockManager _blockManager;
    [SerializeField]
    private ShooterManager _shooterManager;
    [SerializeField]
    private int _startingLevel = 1;

    private void Start() 
    {
        CurrentLevel = _startingLevel;
        StartLevel();
    }

    public void GoToNextLevel()
    {
        CurrentLevel++;
        ClearLevel();
        StartLevel();
    }

    private void ClearLevel()
    {
        CurrentLevelProgress = 0f;
        _blockManager.Clear();
        _shooterManager.Clear();
    }

    private void StartLevel()
    {
        var levelInfo = _levelLoader.LoadLevel(CurrentLevel);
        // Setup block grid
        _blockManager.Setup(levelInfo);
        // Setup shooters
        _shooterManager.Setup(this, levelInfo);
        _blockManager.OnBlockGridUpdated += OnBlockGridUpdated;

        OnLevelStarted?.Invoke();
    }

    private void OnDestroy() 
    {
        _blockManager.OnBlockGridUpdated -= OnBlockGridUpdated;
    }

    public List<Block> GetShootableBlocks()
    {
        return _blockManager.GetShootableBlocks();
    }

    private void OnBlockGridUpdated()
    {
        CurrentLevelProgress = 1f - _blockManager.CurrentNumOfBlocks / (float)_blockManager.MaxNumOfBlocks;
        OnLevelProgressUpdated?.Invoke();
        CheckWinLossCondition();
    }

    private void CheckWinLossCondition()
    {
        // If the bottom row of blocks are not targettable by any shooter then end game with loss

        // if progress is 1, then end game with a win
        if (CurrentLevelProgress >= 1f)
        {
            OnLevelEnded?.Invoke(1);
        }
    }
}
