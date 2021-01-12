using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

[Serializable]
public class LineSegment2D
{
	public Vector2 start;
	public Vector2 end;
	public static LineSegment2D NULL = new LineSegment2D(VectorExtensions.NULL2, VectorExtensions.NULL2);

	public LineSegment2D ()
	{
	}

	public LineSegment2D (Vector2 start, Vector2 end)
	{
		this.start = start;
		this.end = end;
	}

	public override string ToString ()
	{
		return "[" + start + "], [" + end + "]";
	}
	
	public virtual float GetSlope ()
	{
		return (end.y - start.y) / (end.x - start.x);
	}
	
	public virtual float GetFacingAngle ()
	{
		return (end - start).GetFacingAngle();
	}

	public virtual bool DoIIntersectWithLineSegment (LineSegment2D other, bool shouldIncludeEndPoints)
	{
		bool output = false;
		float denominator = (other.end.y - other.start.y) * (end.x - start.x) - (other.end.x - other.start.x) * (end.y - start.y);
		if (denominator != 0f)
		{
			float u_a = ((other.end.x - other.start.x) * (start.y - other.start.y) - (other.end.y - other.start.y) * (start.x - other.start.x)) / denominator;
			float u_b = ((end.x - start.x) * (start.y - other.start.y) - (end.y - start.y) * (start.x - other.start.x)) / denominator;
			if (shouldIncludeEndPoints)
			{
				if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
					output = true;
			}
			else
			{
				if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
					output = true;
			}
		}
		return output;
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
		Gizmos.DrawRay(start, end - start);
	}
#endif

	public virtual bool DoIIntersectWithCircle (Circle2D circle)
	{
		return (ClosestPoint(circle.center) - circle.center).sqrMagnitude <= circle.radius * circle.radius;
	}

	// public virtual bool DoIIntersectWithCircle (Circle2D circle)
	// {
	// 	return Vector2.Distance(GetPointWithDirectedDistance(GetDirectedDistanceAlongParallel(circle.center)), circle.center) <= circle.radius;
	// }

	// public virtual bool DoIIntersectWithCircle2D (Circle2D circle)
	// {
	// 	Vector2 lineDirection = GetDirection();
	// 	Vector2 centerToLineStart = start - circle.center;
	// 	float a = Vector2.Dot(lineDirection, lineDirection);
	// 	float b = 2 * Vector2.Dot(centerToLineStart, lineDirection);
	// 	float c = Vector2.Dot(centerToLineStart, centerToLineStart) - circle.radius * circle.radius;
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
	
	public virtual bool ContainsPoint (Vector2 point)
	{
		return Vector2.Distance(point, start) + Vector2.Distance(point, end) == Vector2.Distance(start, end);
	}
	
	public virtual LineSegment2D Move (Vector2 movement)
	{
		return new LineSegment2D(start + movement, end + movement);
	}
	
	public virtual LineSegment2D Rotate (Vector2 pivotPoint, float degrees)
	{
		LineSegment2D output;
		Vector2 outputStart = start.Rotate(pivotPoint, degrees);
		Vector2 outputEnd = end.Rotate(pivotPoint, degrees);
		output = new LineSegment2D(outputStart, outputEnd);
		return output;
	}

	public virtual Vector2 ClosestPoint (Vector2 point)
	{
		Vector2 output;
		float directedDistanceAlongParallel = GetDirectedDistanceAlongParallel(point);
		if (directedDistanceAlongParallel > 0 && directedDistanceAlongParallel < GetLength())
			output = GetPointWithDirectedDistance(directedDistanceAlongParallel);
		else if (directedDistanceAlongParallel >= GetLength())
			output = end;
		else
			output = start;
		return output;
	}

	public virtual LineSegment2D GetPerpendicular (bool rotateClockwise = false)
	{
		if (rotateClockwise)
			return Rotate(GetMidpoint(), -90);
		else
			return Rotate(GetMidpoint(), 90);
	}

	public virtual Vector2 GetMidpoint ()
	{
		return (start + end) / 2;
	}

	public virtual float GetDirectedDistanceAlongParallel (Vector2 point)
	{
		float rotate = -GetFacingAngle();
		LineSegment2D rotatedLine = Rotate(Vector2.zero, rotate);
		point = point.Rotate(rotate);
		return point.x - rotatedLine.start.x;
	}

	public virtual Vector2 GetPointWithDirectedDistance (float directedDistance)
	{
		return start + (GetDirection() * directedDistance);
	}

	public virtual float GetLength ()
	{
		return Vector2.Distance(start, end);
	}

	public virtual Vector2 GetDirection ()
	{
		return (end - start).normalized;
	}
	
	public virtual LineSegment2D Flip ()
	{
		return new LineSegment2D(end, start);
	}

	public virtual bool GetIntersectionWithLineSegment (LineSegment2D lineSegment, out Vector2 intersection, bool collinearOverlapsIntersect = true)
	{
		intersection = new Vector2();
		Vector2 r = end - start;
		Vector2 s = lineSegment.end - lineSegment.start;
		float rxs = r.Cross(s);
		float qpxr = (lineSegment.start - start).Cross(r);
		if (Mathf.Approximately(rxs, 0) && Mathf.Approximately(qpxr, 0))
			return collinearOverlapsIntersect && ((0 <= (lineSegment.start - start).Multiply_float(r) && (lineSegment.start - start).Multiply_float(r) <= r.Multiply_float(r)) || (0 <= (start - lineSegment.start).Multiply_float(s) && (start - lineSegment.start).Multiply_float(s) <= s.Multiply_float(s)));
		if (Mathf.Approximately(rxs, 0) && !Mathf.Approximately(qpxr, 0))
			return false;
		float t = (lineSegment.start - start).Cross(s) / rxs;
		float u = (lineSegment.start - start).Cross(r) / rxs;
		if (!Mathf.Approximately(rxs, 0) && (0 <= t && t <= 1) && (0 <= u && u <= 1))
		{
			intersection = start + (t * r);
			return true;
		}
		return false;
	}

	// public virtual LineSegment2D[] GetErasedLineSegment (LineSegment2D eraser, Vector2 movement)
	// {
	// 	LineSegment2D[] output = new LineSegment2D[1] { this };
	// 	LineSegment2D eraserStartMovementLine = new LineSegment2D(eraser.start, eraser.start + movement);
	// 	LineSegment2D eraserEndMovementLine = new LineSegment2D(eraser.end, eraser.end + movement);
	// 	Vector2 eraserStartIntersection;
	// 	Vector2 eraserEndIntersection;
	// 	Vector2 eraserStartMovementIntersection;
	// 	Vector2 eraserEndMovementIntersection;
	// 	bool eraserStartIntersects = GetIntersectionWithLineSegment(eraser.start, out eraserStartIntersection);
	// 	bool eraserEndIntersects = GetIntersectionWithLineSegment(eraser.end, out eraserEndIntersection);
	// 	bool eraserStartMovementIntersects = GetIntersectionWithLineSegment(eraserStartMovementLine, out eraserStartMovementIntersection);
	// 	bool eraserEndMovementIntersects = GetIntersectionWithLineSegment(eraserEndMovementLine, out eraserEndMovementIntersection);
	// 	int intersectionCount = 0;
	// 	if (eraserStartIntersects)
	// 		intersectionCount ++;
	// 	if (eraserEndIntersects)
	// 		intersectionCount ++;
	// 	if (eraserStartMovementIntersects)
	// 		intersectionCount ++;
	// 	if (eraserEndMovementIntersects)
	// 		intersectionCount ++;
	// 	if (intersectionCount == 2)
	// 	{
			
	// 	}
	// 	else if (intersectionCount == 1)
	// 	{

	// 	}
	// 	else
	// 	{
			
	// 	}
	// 	return output;
	// }
}