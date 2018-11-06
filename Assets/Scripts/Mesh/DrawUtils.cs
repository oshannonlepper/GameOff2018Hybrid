using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawContext
{

	private static Vector3 ZeroVector3D = Vector3.zero;
	private static Vector2 ZeroVector2D = Vector2.zero;

	private List<Vector3> _vertices;
	private List<Vector3> _normals;
	private List<int> _indices;
	private List<Vector2> _uvs;
	private int _currentMaxIndex = -1;

	public DrawContext()
	{
		_vertices = new List<Vector3>();
		_normals = new List<Vector3>();
		_indices = new List<int>();
		_uvs = new List<Vector2>();
	}

	public DrawContext(Mesh mesh)
	{
		_vertices = mesh.vertices.ToList<Vector3>();
		_normals = mesh.normals.ToList<Vector3>();
		_indices = mesh.GetIndices(0).ToList<int>();
		_uvs = mesh.uv.ToList<Vector2>();

		if (_indices.Count > 0)
		{
			_currentMaxIndex = _indices.Max();
		}
	}

	public DrawContext(DrawContext inCopy)
	{
		_vertices = inCopy._vertices;
		_normals = inCopy._normals;
		_indices = inCopy._indices;
		_uvs = inCopy._uvs;
		_currentMaxIndex = inCopy._currentMaxIndex;
	}

	/** Write this draw context out to a mesh and return it. */

	public Mesh ToMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = _vertices.ToArray();
		mesh.normals = _normals.ToArray();
		mesh.uv = _uvs.ToArray();
		mesh.SetIndices(_indices.ToArray(), MeshTopology.Triangles, 0);
		return mesh;
	}

	/** Combine two draw contexts' data. */

	public static DrawContext operator +(DrawContext lhs, DrawContext rhs)
	{
		DrawContext sum = new DrawContext(lhs);

		sum._vertices.AddRange(rhs._vertices);
		sum._normals.AddRange(rhs._normals);

		List<int> indicesPlus = new List<int>(lhs._indices);
		indicesPlus.AddRange(rhs._indices);
		if (indicesPlus.Count > lhs._indices.Count)
		{
			int add = Mathf.Max(lhs._currentMaxIndex, 0);
			if (add > 0)
			{
				for (int i = lhs._indices.Count; i < indicesPlus.Count; ++i)
				{
					indicesPlus[i] += add+1;
				}
			}
		}
		sum._indices.AddRange(indicesPlus);

		sum._uvs.AddRange(rhs._uvs);

		return sum;
	}

	/**
	 * AddVertex - Add a given vertex to the draw context.
	 * Contains overloads for specifying normals and uvs.
	 */
	
	public void AddVertex(Vector3 vertex)
	{
		AddVertex(vertex, Vector3.back, ZeroVector2D);
	}

	public void AddVertex(Vector3 vertex, Vector3 normal)
	{
		AddVertex(vertex, normal, ZeroVector2D);
	}

	public void AddVertex(Vector3 vertex, Vector3 normal, Vector2 uv)
	{
		_vertices.Add(vertex);
		_normals.Add(normal);
		_uvs.Add(uv);
	}

	/**
	 * AddTriangle - Add 3 indices corresponding to existing vertices in the mesh.
	 */

	public void AddTriangle(int i0, int i1, int i2)
	{
		_indices.Add(i0);
		_indices.Add(i1);
		_indices.Add(i2);
		int imax = Mathf.Max(new int[] { i0, i1, i2 });
		if (imax > _currentMaxIndex)
		{
			_currentMaxIndex = imax;
		}
	}

}

/**
 * Utility functions for easily writing to a mesh.
 */
public class DrawUtils
{

	/**
	 * Base draw utility function, given a mesh and a list of draw contexts,
	 * write the draw context data to the given mesh.
	 */
	public static void DrawToMesh(ref Mesh mesh, List<DrawContext> draws)
	{
		DrawContext sum = new DrawContext(mesh);
		foreach (DrawContext context in draws)
		{
			sum += context;
		}
		mesh = sum.ToMesh();
	}

	/**
	 * Given a mesh, draw a line with rounded edges.
	 * Use startThickness and endThickness to have a line of variable width.
	 * Use LOD to control the detail of the rounded edges.
	 */
	public static void DrawLine(ref Mesh mesh, Vector3 start, Vector3 end, float startThickness, float endThickness, int LOD=16)
	{
		Vector3 direction = (end - start).normalized;
		Vector3 normal = new Vector3(direction.y, -direction.x);

		DrawContext context = new DrawContext();

		context.AddVertex(start);

		// draw two semicircles at start and end, startThickness and endThickness respective radii
		for (int i = 0; i < LOD; ++i)
		{
			float angle = (1.0f*i / LOD) * Mathf.PI;
			context.AddVertex(start - startThickness * ((Mathf.Sin(angle) * direction) + (Mathf.Cos(angle) * normal)));
			if (i < LOD - 1)
			{
				context.AddTriangle(i + 1, i + 2, 0);
			}
		}

		context.AddVertex(end);

		for (int i = 0; i < LOD; ++i)
		{
			float angle = (1.0f*i / LOD) * Mathf.PI;
			context.AddVertex(end + endThickness * ((Mathf.Sin(angle) * direction) - (Mathf.Cos(angle) * normal)));
			if (i < LOD - 1)
			{
				context.AddTriangle(i + LOD + 3, i + LOD + 2, LOD + 1);
			}
		}

		context.AddTriangle(0, LOD, LOD + 1);
		context.AddTriangle(LOD + 2, 0, LOD + 1);

		context.AddTriangle(1, 0, LOD + 2);
		context.AddTriangle((2 * LOD) + 1, LOD + 1, LOD);

		DrawToMesh(ref mesh, new List<DrawContext> { context });
	}

}
