using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Shape2D
{
	public Vector2[] corners;
	public LineSegment2D[] edges;

	public Shape2D (Vector2[] corners)
	{
		this.corners = corners;
		edges = new LineSegment2D[corners.Length - 1];
		for (int i = 0; i < edges.Length; i ++)
			edges[i] = new LineSegment2D(corners[i], corners[i + 1]);
	}

	public Shape2D (LineSegment2D[] edges)
	{
		this.edges = edges;
		corners = new Vector2[edges.Length];
		for (int i = 0; i < edges.Length; i ++)
			corners[i] = edges[i].start;
	}

	public virtual float GetPerimeter ()
	{
		float output = 0;
		foreach (LineSegment2D edge in edges)
			output += edge.GetLength();
		return output;
	}

	public virtual Vector2 GetPointOnPerimeter (float distance)
	{
		float perimeter = GetPerimeter();
		do
		{
			foreach (LineSegment2D edge in edges)
			{
				float edgeLength = edge.GetLength();
				distance -= edgeLength;
				if (distance <= 0)
					return edge.GetPointWithDirectedDistance(edgeLength + distance);
			}
		} while (true);
	}

	public virtual bool Contains (Vector2 point, bool shouldIncludeEndPoints = true, float checkDistance = 99999)
	{
		LineSegment2D checkLineSegment = new LineSegment2D(point, point + (Random.insideUnitCircle.normalized * checkDistance));
		int collisionCount = 0;
		foreach (LineSegment2D edge in edges)
		{
			if (edge.DoIIntersectWithLineSegment(checkLineSegment, shouldIncludeEndPoints))
				collisionCount ++;
		}
		return collisionCount % 2 == 1;
	}

	public virtual Vector2 GetRandomPoint (bool checkIfContained = false)
	{
		float perimeter = GetPerimeter();
		do
		{
			Vector2 point1 = GetPointOnPerimeter(Random.Range(0, perimeter));
			Vector2 point2 = GetPointOnPerimeter(Random.Range(0, perimeter));
			Vector2 output = (point1 + point2) / 2;
			if (Contains(output))
				return output;
		} while (true);
	}
}