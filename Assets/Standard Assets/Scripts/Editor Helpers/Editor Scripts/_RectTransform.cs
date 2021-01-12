#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Extensions;

[RequireComponent(typeof(RectTransform))]
//[ExecuteInEditMode]
public class _RectTransform : EditorScript
{
	public RectTransform rectTrs;
	[HideInInspector]
	public float[] previousValues = new float[0];
	[HideInInspector]
	public float sizeDeltaX;
	[HideInInspector]
	public float sizeDeltaY;
	[HideInInspector]
	public float localPositionX;
	[HideInInspector]
	public float localPositionY;

	public override void OnEnable ()
	{
		base.OnEnable ();
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (rectTrs == null)
				rectTrs = GetComponent<RectTransform>();
			Canvas.ForceUpdateCanvases();
			if (previousValues.Length == 0)
			{
				previousValues = new float[4];
				previousValues[0] = rectTrs.sizeDelta.x;
				previousValues[1] = rectTrs.sizeDelta.y;
				previousValues[2] = rectTrs.localPosition.x;
				previousValues[3] = rectTrs.localPosition.y;
			}
			sizeDeltaX = rectTrs.sizeDelta.x;
			sizeDeltaY = rectTrs.sizeDelta.y;
			localPositionX = rectTrs.localPosition.x;
			localPositionY = rectTrs.localPosition.y;
			SetValueToPrevIfNaN (0, ref sizeDeltaX);
			SetValueToPrevIfNaN (1, ref sizeDeltaY);
			SetValueToPrevIfNaN (2, ref localPositionX);
			SetValueToPrevIfNaN (3, ref localPositionY);
			rectTrs.sizeDelta = rectTrs.sizeDelta.SetX(sizeDeltaX);
			rectTrs.sizeDelta = rectTrs.sizeDelta.SetY(sizeDeltaY);
			rectTrs.localPosition = rectTrs.localPosition.SetX(localPositionX);
			rectTrs.localPosition = rectTrs.localPosition.SetY(localPositionY);
			return;
		}
#endif
	}

	public virtual void SetValueToPrevIfNaN (int index, ref float currentValue, float defaultValue = 0)
	{
		if (float.IsNaN(currentValue))
		{
			if (float.IsNaN(previousValues[index]))
				previousValues[index] = defaultValue;
			currentValue = previousValues[index];
		}
		else
			previousValues[index] = currentValue;
	}
}
#endif
#if !UNITY_EDITOR
public class _RectTransform : EditorScript
{
}
#endif