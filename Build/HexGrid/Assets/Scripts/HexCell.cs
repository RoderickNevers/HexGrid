using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] private GameObject selectedAsset;
    [SerializeField] private GameObject unselectedAsset;
    [SerializeField] private GameObject highlightedAsset;
    [SerializeField] private readonly Dictionary<HexDirection, HexCell> neighbors = new();

    [SerializeField] private bool obstacle;

    private readonly int elevation = int.MinValue;

    public HexCell PathFrom { get; set; }
    public HexCell NextWithSamePriority { get; set; }
    public HexCoordinates Coordinates { get; set; }
    public int SearchHeuristic { get; set; }
    public int Distance { get; set; }
    public int SearchPriority => Distance + SearchHeuristic;
    public bool IsWalled { get; set; }
    public bool IsSelected { get; set; }
    public bool HasObstacle { get => obstacle; set => obstacle = value; }

    public HexCell GetNeighbor(HexDirection direction) => neighbors.FirstOrDefault(x => x.Key.Equals(direction)).Value;
    public HexEdgeType GetEdgeType(HexDirection direction) => HexMetrics.GetEdgeType(elevation, neighbors[direction].elevation);
    public HexEdgeType GetEdgeType(HexCell otherCell) => HexMetrics.GetEdgeType(elevation, otherCell.elevation);

    private void Awake()
    {
        DeselectTile();
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors.Add(direction, cell);
        int opposite = (int)direction.Opposite();
        cell.neighbors.Add((HexDirection)opposite, this);
    }

    public void SelectTile()
    {
        selectedAsset.SetActive(true);
        IsSelected = true;
        unselectedAsset.SetActive(false);
        highlightedAsset.SetActive(false);
    }

    public void DeselectTile()
    {
        unselectedAsset.SetActive(true);
        IsSelected = false;
        selectedAsset.SetActive(false);
        highlightedAsset.SetActive(false);
    }

    public void HighlightTile()
    {
        selectedAsset.SetActive(false);
        unselectedAsset.SetActive(false);
        highlightedAsset.SetActive(true);
    }

    public void DeHighlight()
    {
        if (!IsSelected)
        {
            highlightedAsset.SetActive(false);
            unselectedAsset.SetActive(true);
        }
    }
}