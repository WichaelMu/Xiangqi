using UnityEngine;
using MW;

[RequireComponent(typeof(Board))]
public class BoardUI : MonoBehaviour
{

	[SerializeField] Color gridColour;
	[SerializeField] float Scalar = .1f;
	[SerializeField] Material defaultMaterial;

	Board attached;
	const byte Depth = 1;

	void Start()
	{
		attached = GetComponent<Board>();

		MakeGrid();
	}

	#region Setup

	void MakeGrid()
	{
		const string Horizontal = "Grid Line Marker (Horizontal)";
		const string Vertical = "Grid Line Marker (Vertical)";
		const string Diagonal = "Palace Diagonals";

		Vector3[] vertices = new Vector3[6];
		int[] triangles = new int[18];

		Mesh mesh = new Mesh();

		// Horizontal lines.
		for (int i = 0; i < 82; i += 9)
		{
			GameObject newGridLine = MakeNewGridMarker(Horizontal);

			Vector2 left = Board.board[i].Position;
			Vector2 right = Board.board[i + 8].Position;

			Vector3 bottomLeftShared = new Vector3(left.x, left.y - Scalar, Depth);
			Vector3 topRightShared = new Vector3(right.x, right.y + Scalar, Depth);

			int k = 0;
			vertices[k] = bottomLeftShared;
			vertices[k + 1] = new Vector3(left.x, left.y + Scalar, Depth);
			vertices[k + 2] = topRightShared;

			vertices[k + 3] = topRightShared;
			vertices[k + 4] = new Vector3(right.x, right.y - Scalar, Depth);
			vertices[k + 5] = bottomLeftShared;

			for (int o = 0; o < 6; o += 3)
			{
				triangles[o] = o;
				triangles[o + 1] = o + 1;
				triangles[o + 2] = o + 2;
			}

			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();

			SetMeshData(newGridLine, mesh);

			mesh = new Mesh();
		}

		// Vertical lines.
		for (int i = 0; i < 9; ++i)
		{
			GameObject newGridLine = MakeNewGridMarker(Vertical);

			Vector2 bottom;
			Vector2 top;

			// Left and right edges.
			if (i == 0 || i == 8)
			{
				bottom = Board.board[i].Position;
				top = Board.board[i + 81].Position;

				MakeVerticalLine(bottom, top, out mesh, ref vertices, ref triangles);

				SetMeshData(newGridLine, mesh);
			}
			else // Everything else in between is split.
			{
				bottom = Board.board[i].Position;
				top = Board.board[i + 36].Position;

				MakeVerticalLine(bottom, top, out mesh, ref vertices, ref triangles);

				SetMeshData(newGridLine, mesh);

				newGridLine = MakeNewGridMarker(Vertical);

				bottom = Board.board[i + 45].Position;
				top = Board.board[i + 36 + 45].Position;

				MakeVerticalLine(bottom, top, out mesh, ref vertices, ref triangles);

				SetMeshData(newGridLine, mesh);
			}
		}

		MakeVerticalLine(Board.board[3].Position, Board.board[23].Position, out mesh, ref vertices, ref triangles);
		SetMeshData(MakeNewGridMarker(Diagonal), mesh);
		MakeVerticalLine(Board.board[5].Position, Board.board[21].Position, out mesh, ref vertices, ref triangles);
		SetMeshData(MakeNewGridMarker(Diagonal), mesh);
		MakeVerticalLine(Board.board[66].Position, Board.board[86].Position, out mesh, ref vertices, ref triangles);
		SetMeshData(MakeNewGridMarker(Diagonal), mesh);
		MakeVerticalLine(Board.board[68].Position, Board.board[84].Position, out mesh, ref vertices, ref triangles);
		SetMeshData(MakeNewGridMarker(Diagonal), mesh);
	}

	void MakeVerticalLine(Vector3 p1, Vector3 p2, out Mesh mesh, ref Vector3[] vertices, ref int[] triangles)
	{
		mesh = new Mesh();

		Vector3 leftBottomShared = new Vector3(p1.x - Scalar, p1.y, Depth);
		Vector3 leftTopShared = new Vector3(p2.x - Scalar, p2.y, Depth);
		Vector3 rightBottomShared = new Vector3(p1.x + Scalar, p1.y, Depth);

		int k = 0;
		vertices[k] = leftBottomShared;
		vertices[k + 1] = leftTopShared;
		vertices[k + 2] = rightBottomShared;

		vertices[k + 3] = leftTopShared;
		vertices[k + 4] = new Vector3(p2.x + Scalar, p2.y, Depth);
		vertices[k + 5] = rightBottomShared;

		for (int o = 0; o < 6; o += 3)
		{
			triangles[o] = o;
			triangles[o + 1] = o + 1;
			triangles[o + 2] = o + 2;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}

	GameObject MakeNewGridMarker(string Name)
	{
		GameObject newGridLine = new GameObject(Name, typeof(MeshRenderer), typeof(MeshFilter));
		MeshRenderer mr = newGridLine.GetComponent<MeshRenderer>();
		mr.sharedMaterial = defaultMaterial;
		mr.sharedMaterial.color = gridColour;

		// Re-parent.
		newGridLine.transform.parent = transform;

		return newGridLine;
	}

	void SetMeshData(GameObject gameObject, Mesh mesh)
	{
		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		mf.mesh = mesh;
	}

	#endregion

	public void HighlightIntersections(MArray<Point> legal)
	{

	}
}
