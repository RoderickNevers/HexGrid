using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] private Dictionary<HexDirection, HexCell> neighbors = new();

    public HexCoordinates coordinates;
    private int elevation = int.MinValue;

    public HexCell PathFrom { get; set; }
    public HexCell NextWithSamePriority { get; set; }
    public int SearchHeuristic { get; set; }
    public int Distance { get; set; }
    public int SearchPriority => Distance + SearchHeuristic;
    public bool Walled { get; set; }

    public HexCell GetNeighbor(HexDirection direction) => neighbors[direction];
    public HexEdgeType GetEdgeType(HexDirection direction) => HexMetrics.GetEdgeType(elevation, neighbors[direction].elevation);
    public HexEdgeType GetEdgeType(HexCell otherCell) => HexMetrics.GetEdgeType(elevation, otherCell.elevation);

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors.Add(direction, cell);
        int opposite = (int)direction.Opposite();
        cell.neighbors.Add((HexDirection)opposite, this);
    }

    public void DisableHighlight()
    {
        //highlight.enabled = false;
    }

    public void EnableHighlight()
    {
        //highlight.enabled = true;
    }
}