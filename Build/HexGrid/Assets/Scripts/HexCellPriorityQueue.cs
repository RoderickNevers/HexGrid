using System.Collections.Generic;

public class HexCellPriorityQueue
{
    private readonly List<HexCell> hexCells = new();
    private int total = 0;
    private int minimum = int.MaxValue;

	public bool HasContents => total > 0;

	public void Enqueue (HexCell cell)
	{
		total += 1;
		int priority = cell.SearchPriority;

		if (priority < minimum)
		{
			minimum = priority;
		}

		while (priority >= hexCells.Count)
		{
			hexCells.Add(null);
		}

		cell.NextWithSamePriority = hexCells[priority];
		hexCells[priority] = cell;
	}

	public HexCell Dequeue ()
	{
		total -= 1;

		for (int minimum = 0; minimum < hexCells.Count; minimum++)
		{
			HexCell cell = hexCells[minimum];
			if (cell != null)
			{
				hexCells[minimum] = cell.NextWithSamePriority;
				return cell;
			}
		}

		return null;
	}

	public void Change (HexCell cell, int oldPriority)
	{
		HexCell current = hexCells[oldPriority];
		HexCell next = current.NextWithSamePriority;

		if (current == cell)
		{
			hexCells[oldPriority] = next;
		}
		else
		{
			while (next != cell)
			{
				current = next;
				next = current.NextWithSamePriority;
			}

			current.NextWithSamePriority = cell.NextWithSamePriority;
		}

		Enqueue(cell);
		total -= 1;
	}

	public void Clear ()
	{
		hexCells.Clear();
		total = 0;
		minimum = int.MaxValue;
	}
}