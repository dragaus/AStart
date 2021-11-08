using System.Collections.Generic;
using UnityEngine;
using PathFinding;

public class RectGridCellA : Node<Vector2Int>
{
    public bool isWalkable;

    private RectGridBuilder rectGridBuilder;

    public RectGridCellA(RectGridBuilder gridMap, Vector2Int value) : base(value) 
    {
        rectGridBuilder = gridMap;

        isWalkable = true;
    }

    public override List<Node<Vector2Int>> GetNeighbours()
    {
        return rectGridBuilder.GetNeighboursCells(this);
    }
}
