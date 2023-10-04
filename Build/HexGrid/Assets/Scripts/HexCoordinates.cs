using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
	[SerializeField] private readonly int x, z;

	public readonly int X => x;
    public readonly int Y => -X - Z;
    public readonly int Z => z;

	public HexCoordinates (int x, int z)
	{
		this.x = x;
		this.z = z;
	}

    public static HexCoordinates FromOffsetCoordinates (int x, int z) => new HexCoordinates(x - z / 2, z);

    public static HexCoordinates FromPosition (Vector3 position)
	{
		float x = position.x / (HexMetrics.innerRadius * 2f);
		float y = -x;

		float offset = position.z / (HexMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x -y);

		if (iX + iY + iZ != 0)
		{
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x -y - iZ);

			if (dX > dY && dX > dZ)
			{
				iX = -iY - iZ;
			}
			else if (dZ > dY)
			{
				iZ = -iX - iY;
			}
		}

		return new HexCoordinates(iX, iZ);
	}

    public int DistanceTo(HexCoordinates other)
    {
		var xCoord = x < other.x ? other.x - x : x - other.x;
		var yCoord = Y < other.Y ? other.Y - Y : Y - other.Y;
		var zCoord = z < other.z ? other.z - z : z - other.z;

        return (xCoord + yCoord + zCoord) / 2;
    }

    public override string ToString () => $"({X}, {Y}, {Z})";
}