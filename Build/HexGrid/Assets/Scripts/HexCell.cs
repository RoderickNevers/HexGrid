using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] private GameObject selectedAsset;
    [SerializeField] private GameObject unselectedAsset;
    [SerializeField] private readonly Dictionary<HexDirection, HexCell> neighbors = new();

    private readonly int elevation = int.MinValue;

    public HexCell PathFrom { get; set; }
    public HexCell NextWithSamePriority { get; set; }
    public int SearchHeuristic { get; set; }
    public int Distance { get; set; }
    public int SearchPriority => Distance + SearchHeuristic;
    public bool Walled { get; set; }

    public HexCoordinates Coordinates { get; set; }
    public HexCell GetNeighbor(HexDirection direction) => neighbors.FirstOrDefault(x => x.Key.Equals(direction)).Value;
    public HexEdgeType GetEdgeType(HexDirection direction) => HexMetrics.GetEdgeType(elevation, neighbors[direction].elevation);
    public HexEdgeType GetEdgeType(HexCell otherCell) => HexMetrics.GetEdgeType(elevation, otherCell.elevation);

    private void Awake()
    {
        DisableHighlight();
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors.Add(direction, cell);
        int opposite = (int)direction.Opposite();
        cell.neighbors.Add((HexDirection)opposite, this);
    }

    public void DisableHighlight()
    {
        selectedAsset.SetActive(false);
        unselectedAsset.SetActive(true);
    }

    public void EnableHighlight()
    {
        selectedAsset.SetActive(true);
        unselectedAsset.SetActive(false);
    }
}