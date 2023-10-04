using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class HexGrid : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int _Width = 10;
    [SerializeField] private int _Height = 10;

    [Header("Prefabs")]
    [SerializeField] private HexCell _CellPrefab;

    private List<HexCell> cells = new();
    private HexCellPriorityQueue searchFrontier;
    private HexCell startingCell;

    private void Start ()
    {
        SpawnGrid();
    }

	private void SpawnGrid()
	{
        for (int z = 0, i = 0; z < _Height; z++)
        {
            for (int x = 0; x < _Width; x++)
            {
                CreateCell(x, z, i++);
            }
        }

        startingCell = cells.First();
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
                cell.SetNeighbor(HexDirection.SE, cells[i - _Width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - _Width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - _Width]);
                if (x < _Width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - _Width + 1]);
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
        if (Input.GetMouseButton(0))
        {
            TouchCell();
        }
    }

    private void TouchCell()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            HexCell selectedCell = hit.collider.GetComponentInParent<HexCell>();
            if (selectedCell == null)
            {
                return;
            }

            Debug.Log("touched at " + selectedCell.Coordinates.ToString());

            FindPath(startingCell, selectedCell);
            //Vector3 position = transform.InverseTransformPoint(hit.point);
            //HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        }
    }

    public void FindPath(HexCell fromCell, HexCell toCell)
    {
        StopAllCoroutines();
        StartCoroutine(Search(fromCell, toCell));
    }

    IEnumerator Search(HexCell fromCell, HexCell toCell)
    {
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        foreach (HexCell c in cells)
        {
            c.Distance = int.MaxValue;
            c.DisableHighlight();
        }

        fromCell.EnableHighlight();
        toCell.EnableHighlight();

        var delay = new WaitForSeconds(1 / 60f);
        fromCell.Distance = 0;
        searchFrontier.Enqueue(fromCell);

        while (searchFrontier.Count > 0)
        {
            yield return delay;
            HexCell current = searchFrontier.Dequeue();

            if (current == toCell)
            {
                current = current.PathFrom;
                while (current != fromCell)
                {
                    current.EnableHighlight();
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

                HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff)
                {
                    continue;
                }

                int distance = current.Distance;

                if (current.Walled != neighbor.Walled)
                {
                    continue;
                }

                if (neighbor.Distance == int.MaxValue)
                {
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.Coordinates.DistanceTo(toCell.Coordinates);
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