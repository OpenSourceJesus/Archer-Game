#if UNITY_EDITOR
using UnityEngine;
using Extensions;

[RequireComponent(typeof(PolygonCollider2D))]
public class ArcCollider : EditorScript
{
	public Vector2 center;
	[Range(0f, 360f)]
	public float innerDegrees;
	[Range(0f, 360f)]
	public float outerDegrees;
	public float facing;
	public float innerRadius;
	public float outerRadius;
	[Range(2, 50)]
	public int innerPointCount;
	[Range(2, 50)]
	public int outerPointCount;
	public PolygonCollider2D polygonCollider;

	Vector2[] points;

	public override void OnEnable ()
	{
		base.OnEnable ();
		if (Application.isPlaying)
		{
			Destroy(this);
			return;
		}
	}

	int index;
	public override void DoEditorUpdate ()
	{
		index = 0;
		points = new Vector2[innerPointCount + outerPointCount + 2];
		for (float angle = facing - innerDegrees / 2; angle <= facing + innerDegrees / 2; angle += innerDegrees / innerPointCount)
		{
			points[index] = center + VectorExtensions.FromFacingAngle(angle) * innerRadius;
			index ++;
		}
		for (float angle = facing + outerDegrees / 2; angle >= facing - outerDegrees / 2; angle -= outerDegrees / outerPointCount)
		{
			points[index] = center + VectorExtensions.FromFacingAngle(angle) * outerRadius;
			index ++;
		}
		polygonCollider.points = points;
	}
}
#endif