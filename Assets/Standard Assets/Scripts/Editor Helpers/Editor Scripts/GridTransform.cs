#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class GridTransform : EditorScript
{
	public Transform trs;
	protected Vector2 offset;
	public float smallValue = 0.0001f;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			return;
		}
	}

	public override void DoEditorUpdate ()
	{
		trs.SetWorldScale(trs.lossyScale.Snap(Vector2.one));
		if (trs.localScale.x % 2 == 0)
			offset.x = -.5f + smallValue;
		else
			offset.x = 0;
		if (trs.localScale.y % 2 == 0)
			offset.y = 0;
		else
			offset.y = -.5f + smallValue;
		trs.position = trs.position.Snap(Vector2.one) + (Vector3) offset;
	}
}
#endif
#if !UNITY_EDITOR
public class GridTransform : EditorScript
{
}
#endif