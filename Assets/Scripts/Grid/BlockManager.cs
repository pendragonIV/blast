using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public event Action OnBlockGridUpdated;
    public int CurrentNumOfBlocks => _blocks.Count;
    public int MaxNumOfBlocks => _maxBlocks;

    [SerializeField]
    private EntityGrid _blockGrid;
    [SerializeField]
    private Block _blockPrefab;
    [SerializeField]
    private Block _pigBlockPrefab;
    private List<Block> _blocks = new();
    private int _maxBlocks;

    public void Setup(LevelInfo levelInfo)
    {
        SetupGrid(levelInfo);
    }

    public List<Block> GetShootableBlocks()
    {
        return _blocks.Where(block => IsInRow(0, block)).OrderBy(block => block.GridNodes[0].GridCoordinate.y).ToList();
    }

    public void Clear()
    {
        for (int i = 0; i < _blocks.Count; i++)
        {
            Destroy(_blocks[i].gameObject);
        }

        _blocks.Clear();
        _blockGrid.ClearGrid();
    }
    
    private void SetupGrid(LevelInfo levelInfo)
    {
        var levelGrid = levelInfo.LevelGrid;
        var rows = levelGrid.Count;
        var cols = levelGrid[0].Length;
        _blockGrid.SetupGrid(new Vector2(rows, cols));
        _maxBlocks = rows * cols;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var gridNode = _blockGrid.GetGridNode(new Vector2(i, j));
                gridNode.IsOccupied = true;
                var colour = ColourUtils.GetColorFromHex(levelGrid[i][j]);
                if (colour == Color.white)
                    continue;

                Block block;
                if (colour == Color.black)
                {
                    block = Instantiate(_pigBlockPrefab, _blockGrid.transform);
                }
                else
                {
                    block = Instantiate(_blockPrefab, _blockGrid.transform);
                }
                block.Setup(new List<GridNode>(){gridNode}, colour, levelInfo.NumOfBlockLayers, 1);
                block.OnDie += OnBlockDied;
                _blocks.Add(block);
            }
        }
    }

    private void OnBlockDied(Block block)
    {
        UpdateBlockGrid(block);
    }

    private void UpdateBlockGrid(Block blockToRemove)
    {
        _blocks.Remove(blockToRemove);

        var affectedColumns = new List<int>();
        var affectedRows = new List<int>();
        for (int i = 0; i < blockToRemove.GridNodes.Count; i++)
        {
            blockToRemove.GridNodes[i].IsOccupied = false;
            affectedColumns.Add((int)blockToRemove.GridNodes[i].GridCoordinate.y);
            affectedRows.Add((int)blockToRemove.GridNodes[i].GridCoordinate.x);
        }

        for (int j = 0; j < _blocks.Count; j++)
        {
            var block = _blocks[j];
            if (IsBlockInColumns(block, affectedColumns))
            {
                UpdateGridNodesByRows(block, affectedRows.Count);
            }
        }
        
        if (blockToRemove != null)
            Destroy(blockToRemove.gameObject);
        OnBlockGridUpdated?.Invoke();
    }

    private bool IsInRow(int row, Block block)
    {
        var gridNodes = block.GridNodes;

        for (int i = 0; i < gridNodes.Count; i++)
        {
            if (gridNodes[i].GridCoordinate.x == row)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockInColumns(Block block, List<int> columns)
    {
        for (int i = 0; i < block.GridNodes.Count; i++)
        {
            var nodeY = (int)block.GridNodes[i].GridCoordinate.y;
            if (columns.Contains(nodeY))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateGridNodesByRows(Block block, int numOfRows)
    {
        var gridNodes = block.GridNodes;
        var newGridNodes = new List<GridNode>();
        for (int i = 0; i < gridNodes.Count; i++)
        {
            var oldGridNode = gridNodes[i];
            oldGridNode.IsOccupied = false;

            var newGridNode = _blockGrid.GetGridNode(new Vector2(oldGridNode.GridCoordinate.x - numOfRows, oldGridNode.GridCoordinate.y));

            newGridNodes.Add(newGridNode);
        }

        block.MoveAndUpdateGridNodes(newGridNodes);
    }
}
