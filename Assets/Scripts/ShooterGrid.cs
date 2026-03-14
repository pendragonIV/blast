using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShooterGrid : EntityGrid
{
    [SerializeField]
    private GameObject _slotPrefab;
    private List<GameObject> _slots = new();

    public void Setup(Vector2 gridSize, float cellGap)
    {
        SetCellGap(cellGap);
        SetupGrid(gridSize);
        SetupSlots();
    }

    public void Clear()
    {
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
