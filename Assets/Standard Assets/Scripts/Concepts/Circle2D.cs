using System;
using UnityEngine;
using Extensions;
using ArcherGame;

[Serializable]
public class Circle2D
{
    public Vector2 center;
    public float radius;
    public float Diameter
    {
        get
        {
            return radius * 2;
        }
        set
        {
            radius = value / 2;
        }
    }
	public float Circumference
	{
		get
		{
			return Mathf.PI * radius * 2;
		}
		set
		{
			radius = value / (Mathf.PI * 2);
		}
	}

	public Circle2D ()
	{
	}

	public Circle2D (Vector2 center, float radius)
	{
		this.center = center;
		this.radius = radius;
	}

	public virtual Vector2[] GetPointsAlongOutside (float addToAngle, float startAngle = 0)
	{
		Vector2[] output = new Vector2[0];
		float currentAngle = startAngle;
		do
		{
			output = output.Add(GetPointAtAngle(currentAngle));
			currentAngle += addToAngle;
		} while (Mathf.Abs(currentAngle - startAngle) <= 360f - Mathf.Abs(addToAngle));
		return output;
	}

	public virtual Vector2 GetPointAtAngle (float angle)
	{
		return center + VectorExtensions.FromFacingAngle(angle) * radius;
	}

	public bool DoIIntersectWithRect (Rect rect)
	{
		foreach (LineSegment2D edge in rect.GetEdges())
		{
			if (edge.DoIIntersectWithCircle(this))
				return true;
		}
		return false;
	}

#if UNITY_EDITOR
	public virtual void DrawGizmos (Color color)
	{
		GizmosManager.GizmosEntry gizmosEntry = new GizmosManager.GizmosEntry();
		gizmosEntry.setColor = true;
		gizmosEntry.color = color;
		gizmosEntry.onDrawGizmos += DrawGizmos;
		GizmosManager.gizmosEntries.Add(gizmosEntry);
	}

	public virtual void DrawGizmos (params object[] args)
	{
		Gizmos.DrawSphere(center, radius);
	}
#endif

	// public bool DoIIntersectWithLineSegment2D (LineSegment2D lineSegment)
	// {
	// 	Vector2 lineDirection = lineSegment.GetDirection();
	// 	Vector2 centerToLineStart = lineSegment.start - center;
	// 	float a = Vector2.Dot(lineDirection, lineDirection);
	// 	float b = 2 * Vector2.Dot(centerToLineStart, lineDirection);
	// 	float c = Vector2.Dot(centerToLineStart, centerToLineStart) - radius * radius;
	// 	float discriminant = b * b - 4 * a * c;
	// 	if (discriminant >= 0)
	// 	{
	// 		discriminant = Mathf.Sqrt(discriminant);
	// 		float t1 = (-b - discriminant) / (2 * a);
	// 		float t2 = (-b + discriminant) / (2 * a);
	// 		if (t1 >= 0 && t1 <= 1 || t2 >= 0 && t2 <= 1)
	// 			return true;
	// 	}
	// 	return false;
	// }
}