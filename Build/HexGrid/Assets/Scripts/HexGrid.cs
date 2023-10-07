using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int width = 11;
    [SerializeField] private int height = 11;

    [Header("Prefabs")]
    [SerializeField] private HexCell _CellPrefab;

    private List<HexCell> cells = new();
    private HexCellPriorityQueue searchFrontier;
    private HexCell startingCell;
    private GamePlayerPhase gamePlayerPhase = GamePlayerPhase.Build;
    private readonly List<HexDirection> pattern = new()
        {
            HexDirection.W,
            HexDirection.NW,
            HexDirection.NE,
            HexDirection.E
        };

    private void Start ()
    {
        SpawnGrid();
    }

	private void SpawnGrid()
	{
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }

        startingCell = cells.First();
        startingCell.UseCell();
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position = SetGridPosition(x, z);

        HexCell cell = Instantiate(_CellPrefab);
        cell.transform.SetParent(transform, worldPositionStays: false);
        cell.transform.localPosition = position;
        cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cells.Add(cell);

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                }
            }
        }
    }

    private Vector3 SetGridPosition(int x, int z)
    {
        Vector3 position;

        // As it is more convenient to work with rectangular grids, let's force the cells back in line. We do this by
        // undoing part of the offset. Every second row, all cells should move back one additional step. Subtracting
        // the integer division of Z by 2 before multiplying will do the trick.
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        return position;
    }

    private void Update()
    {
        switch (gamePlayerPhase)
        {
            case GamePlayerPhase.Build:
                MouseOverTile();
                break;

            case GamePlayerPhase.Move:
                if (Input.GetMouseButtonDown(0))
                {
                    TouchTile();
                }
                break;

            case GamePlayerPhase.Explore:
                break;

            case GamePlayerPhase.Visit:
                break;
        }
    }

    private void MouseOverTile()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            HexCell selectedCell = hit.collider.GetComponentInParent<HexCell>();
            if (selectedCell == null || startingCell == selectedCell || selectedCell.IsSelected)
            {
                return;
            }

            HighlightShape(selectedCell);
        }
    }

    private void HighlightShape(HexCell selectedCell)
    {
        var roughSelected = selectedCell.Neighbours.Where(x =>
            x.Key.Equals(HexDirection.W) && x.Value.IsValid ||
            x.Key.Equals(HexDirection.NW) && x.Value.IsValid ||
            x.Key.Equals(HexDirection.NE) && x.Value.IsValid ||
            x.Key.Equals(HexDirection.E) && x.Value.IsValid);

        var cleanedSelected = new Dictionary<HexDirection, HexCell>();
        var cleanedCellsOnly = new List<HexCell>();

        foreach (KeyValuePair<HexDirection, HexCell> cell in roughSelected)
        {
            cleanedSelected.Add(cell.Key, cell.Value);
            cleanedCellsOnly.Add(cell.Value);
        }

        int total = 0;
        foreach (KeyValuePair<HexDirection, HexCell> cell in cleanedSelected)
        {
            foreach (HexDirection pattern in pattern)
            {
                if (cell.Key.Equals(pattern))
                {
                    total++;
                    break;
                }
            }
        }

        if (total != pattern.Count)
        {
            return;
        }

        cleanedCellsOnly.Add(selectedCell);

        cleanedCellsOnly.ForEach(cell => cell.HighlightTile());

        var notAlone = cleanedCellsOnly.Any(cell => cell.Neighbours.Any(n => n.Value.IsUsed));

        // Handle selecting the tiles/cells
        if (Input.GetMouseButtonDown(0) && notAlone)
        {
            cleanedCellsOnly.ForEach(cell => cell.UseCell());
        }

        var dups = new List<HexCell>();
        cleanedCellsOnly.ForEach(hexcell => dups.Add(hexcell));

        IEnumerable<HexCell> others = cells.Except(dups);

        foreach (HexCell cell in others)
        {
            if (cell.IsUsed)
            {
                continue;
            }

            cell.DeHighlight();
        }
    }

    private void TouchTile()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            HexCell selectedCell = hit.collider.GetComponentInParent<HexCell>();
            if (selectedCell == null)
            {
                return;
            }

            //Debug.Log("touched at " + selectedCell.Coordinates.ToString());

            switch(gamePlayerPhase)
            {
                case GamePlayerPhase.Move:
                    if (startingCell == selectedCell)
                    {
                        return;
                    }

                    FindPath(startingCell, selectedCell);
                    break;
            }
        }
    }

    private void FindPath(HexCell fromCell, HexCell toCell)
    {
        StopAllCoroutines();
        StartCoroutine(Search(fromCell, toCell));
    }

    private IEnumerator Search(HexCell startingCell, HexCell DestinationCell)
    {
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        foreach (HexCell cell in cells)
        {
            cell.Distance = int.MaxValue;
            cell.DeselectTile();
        }

        startingCell.SelectTile();
        DestinationCell.SelectTile();

        var delay = new WaitForSeconds(1 / 60f);
        startingCell.Distance = 0;
        searchFrontier.Enqueue(startingCell);

        while (searchFrontier.HasContents)
        {
            yield return delay;
            HexCell current = searchFrontier.Dequeue();

            if (current == DestinationCell)
            {
                current = current.PathFrom;
                while (current != startingCell)
                {
                    current.SelectTile();
                    current = current.PathFrom;
                }

                break;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null)
                {
                    continue;
                }

                if (neighbor.HasObstacle)
                {
                    continue;
                }

                int distance = current.Distance;

                if (current.IsWalled != neighbor.IsWalled)
                {
                    continue;
                }

                if (neighbor.Distance == int.MaxValue)
                {
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.Coordinates.DistanceTo(DestinationCell.Coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }
    }
}