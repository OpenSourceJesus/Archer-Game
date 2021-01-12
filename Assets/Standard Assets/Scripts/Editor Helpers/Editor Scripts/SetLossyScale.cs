#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class SetLossyScale : EditorScript
{
	public Transform trs;
	public Vector3 scale;

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
		trs.SetWorldScale(scale);
	}
}
#endif
#if !UNITY_EDITOR
public class SetLossyScale : EditorScript
{
}
#endif