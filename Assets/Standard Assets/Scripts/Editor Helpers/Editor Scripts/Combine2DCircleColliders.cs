#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;
using UnityEditor;

//[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(PolygonCollider2D))]
public class Combine2DCircleColliders : EditorScript
{
	public PolygonCollider2D polygonCollider;
	public CircleCollider2D[] circleColliders;
	public float angleInterval;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (polygonCollider == null)
				polygonCollider = GetComponent<PolygonCollider2D>();
			return;
		}
	}

	public virtual void Do ()
	{
		Vector2[] points;
		Circle2D circle;
		Transform circleColliderTrs;
		Vector2[] previousPoints;
		LineSegment2D[] lineSegments;
		Dictionary<int, LineSegment2D[]> previousPathsLineSegments = new Dictionary<int, LineSegment2D[]>();
		LineSegment2D lineSegment;
		LineSegment2D previousLineSegment;
		LineSegment2D[] previousLineSegments;
		polygonCollider.pathCount = 0;
		foreach (CircleCollider2D circleCollider in circleColliders)
		{
			circleColliderTrs = circleCollider.GetComponent<Transform>();
			circle = new Circle2D((Vector2) circleColliderTrs.position + circleCollider.offset * circleColliderTrs.lossyScale.x, circleCollider.radius * circleColliderTrs.lossyScale.x);
			points = circle.GetPointsAlongOutside(angleInterval);
			previousPathsLineSegments.Clear();
			for (int i = 0; i < polygonCollider.pathCount; i ++)
			{
				previousPoints = polygonCollider.GetPath(i);
				previousPathsLineSegments.Add(i, new LineSegment2D[0]);
				for (int i2 = 1; i2 < previousPoints.Length; i2 ++)
					previousPathsLineSegments[i] = previousPathsLineSegments[i].Add(new LineSegment2D(previousPoints[i2 - 1], previousPoints[i2]));
			}
			lineSegments = new LineSegment2D[0];
			for (int i = 1; i < points.Length; i ++)
			{
				lineSegment = new LineSegment2D(points[i - 1], points[i]);
				lineSegments = lineSegments.Add(lineSegment);
				for (int i2 = 0; i2 < previousPathsLineSegments.Keys.Count; i2 ++)
				{
					previousLineSegments = previousPathsLineSegments[i2];
					for (int i3 = 0; i3 < previousLineSegments.Length; i3 ++)
					{
						previousLineSegment = previousLineSegments[i3];
						if (lineSegment.DoIIntersectWithLineSegment(previousLineSegment, false))
						{
							// TODO: Finish this part
						}
					}
				}
			}
			polygonCollider.pathCount ++;
			polygonCollider.SetPath(polygonCollider.pathCount - 1, points);
		}
	}
}

[CustomEditor(typeof(Combine2DCircleColliders))]
public class Combine2DCircleCollidersEditor : EditorScriptEditor
{
}
#endif