#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;
using UnityEditor;

//[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
public class MakeCirclePolygonCollider2D : EditorScript
{
	public Transform trs;
	public PolygonCollider2D polygonCollider;
	public Circle2D circle = new Circle2D();
	[Range(3, 50)]
	public int pointCount;
	Vector2[] points;

	public virtual void Awake ()
	{
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			if (polygonCollider == null)
				polygonCollider = GetComponent<PolygonCollider2D>();
			return;
		}
	}

	public virtual void Do ()
	{
		points = new Vector2[pointCount];
		for (int i = 0; i < pointCount; i ++)
			points[i] = circle.center + VectorExtensions.FromFacingAngle((i * 360f / pointCount) * (1f / 360f) * 360) * circle.radius;
		polygonCollider.points = points;
	}
}

[CustomEditor(typeof(MakeCirclePolygonCollider2D))]
public class MakeCirclePolygonCollider2DEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class MakeCirclePolygonCollider2D : EditorScript
{
}
#endif