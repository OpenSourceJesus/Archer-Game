#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class GridRectTransform : GridTransform
{
	public RectTransform rectTrs;

	public override void Start ()
	{
        base.Start ();
		if (!Application.isPlaying)
		{
			if (rectTrs == null)
				rectTrs = GetComponent<RectTransform>();
			return;
		}
	}

	public override void DoEditorUpdate ()
	{
        base.DoEditorUpdate ();
        rectTrs.sizeDelta = rectTrs.sizeDelta.Snap(Vector2.one);
        if (rectTrs.sizeDelta.x % 2 == 0)
            offset.x = .5f - smallValue;
        else
            offset.x = 0;
        if (rectTrs.sizeDelta.y % 2 == 0)
            offset.y = 0;
        else
            offset.y = .5f - smallValue;
        rectTrs.position = rectTrs.position.Snap(Vector2.one) + (Vector3) offset;
	}
}
#endif
#if !UNITY_EDITOR
public class GridRectTransform : GridTransform
{
}
#endif