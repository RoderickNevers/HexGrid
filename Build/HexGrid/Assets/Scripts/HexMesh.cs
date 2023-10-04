using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

    private List<Vector3> _Vertices;
    private List<Color> _Colors;
    private List<int> _Triangles;
    private Mesh _HexMesh;
    private MeshCollider _MeshCollider;

    private void Awake ()
	{
        _HexMesh = new Mesh();
		GetComponent<MeshFilter>().mesh = _HexMesh;

        _MeshCollider = gameObject.AddComponent<MeshCollider>();

		_HexMesh.name = "HexCell";

		_Vertices = new List<Vector3>();
		_Colors = new List<Color>();
		_Triangles = new List<int>();
	}

    public void Triangulate (IEnumerable<HexCell> cells)
	{
		_HexMesh.Clear();
		_Vertices.Clear();
		_Colors.Clear();
		_Triangles.Clear();

		foreach (HexCell cell in cells)
		{
            CreateTriangle(cell);
        }

		_HexMesh.vertices = _Vertices.ToArray();
		_HexMesh.colors = _Colors.ToArray();
		_HexMesh.triangles = _Triangles.ToArray();
		_HexMesh.RecalculateNormals();
		_MeshCollider.sharedMesh = _HexMesh;
	}

	private void CreateTriangle (HexCell cell)
	{
		Vector3 center = cell.transform.localPosition;
		
		for (int i = 0; i < HexMetrics.corners.Count; i++)
		{
			int ends = i + 1 >= HexMetrics.corners.Count ? 0 : i+1;
            AddTriangle(center, center + HexMetrics.corners[i], center + HexMetrics.corners[ends]);
        }
	}

    private void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = _Vertices.Count;

		_Vertices.Add(v1);
		_Vertices.Add(v2);
		_Vertices.Add(v3);
		_Triangles.Add(vertexIndex);
		_Triangles.Add(vertexIndex + 1);
		_Triangles.Add(vertexIndex + 2);
	}
}