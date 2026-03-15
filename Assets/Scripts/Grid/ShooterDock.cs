using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShooterDock : EntityGrid
{
    [SerializeField]
    private GameObject _slotPrefab;
    private List<Shooter> _shooters = new();
    private List<GameObject> _slots = new();

    public void Setup(int numOfColumns, float cellGap)
    {
        SetCellGap(cellGap);
        SetupGrid(new Vector2(1, numOfColumns));
        SetupSlots();
    }

    public bool TryGetEmptyGridNode(out GridNode emptyGridNode)
    {
        emptyGridNode = GetEmptyGridNode();
        if (emptyGridNode == null)
        {
            return false;
        }

        return true;
    }

    public bool TryGetAdjacentEmptyGridNodes(out List<GridNode> emptyGridNodes)
    {
        emptyGridNodes = new();
        var allEmptyGridNodes = GetAllEmptyGridNodes();

        for (int i = 0; i < allEmptyGridNodes.Count - 1; i++)
        {
            var currentNode = allEmptyGridNodes[i];
            var nextNode = allEmptyGridNodes[i + 1];
            
            if (nextNode.GridCoordinate.y == currentNode.GridCoordinate.y + 1)
            {
                emptyGridNodes.Add(currentNode);
                emptyGridNodes.Add(nextNode);
                return true;
            }
        }

        return false;
    }

    public void AddShooter(Shooter shooter, GridNode targetGridNode)
    {
        _shooters.Add(shooter);
        shooter.IsUninteractable = true;
    }

    public void RemoveShooter(Shooter shooter)
    {
        _shooters.Remove(shooter);
    }

    public bool TryGetMergeableShooters(Color colour, out List<Shooter> mergeableShooters)
    {
        mergeableShooters = _shooters.Where(shooter => shooter.Colour == colour && shooter.IsAlive && !shooter.IsMerged && !shooter.HasConnection).ToList();
        if (mergeableShooters.Count >= 3)
        {
            mergeableShooters = mergeableShooters.OrderBy(shooter => shooter.GridNode.GridCoordinate.y).ToList();
            return true;
        }
        else
        {
            mergeableShooters = null;
            return false;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _shooters.Count; i++)
        {
            _shooters[i].IsAlive = false;
            Destroy(_shooters[i].gameObject);
        }

        _shooters.Clear();
        ClearGrid();
        ClearSlots();
    }

    private void SetupSlots()
    {
        var gridNodes = _gridNodes.Values.ToList();
        for (int i = 0; i < gridNodes.Count; i++)
        {
            var slot = Instantiate(_slotPrefab, transform);
            slot.transform.position = gridNodes[i].WorldPos;
            _slots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            Destroy(_slots[i]);
        }
        _slots.Clear();
    }
}
