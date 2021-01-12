#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Events;
using Extensions;

public class DisplayFurthestLocalPositionInAnimation : EditorScript
{
	public Result result;
	public AnimationManager animationManager;
	Vector3 furthestLocalPosition;
	Vector3 localPosition;
	float furthestDistanceToLocalPosition;
	float distanceToLocalPosition;
	int keyFrameIndex;

	public override void DoEditorUpdate ()
	{
		furthestLocalPosition = animationManager.CurrentAnim.keyFrames[0].localPosition;
		furthestDistanceToLocalPosition = furthestLocalPosition.magnitude;
		keyFrameIndex = 0;
		for (int i = 1; i < animationManager.CurrentAnim.keyFrames.Count; i ++)
		{
			localPosition = animationManager.CurrentAnim.keyFrames[i].localPosition;
			distanceToLocalPosition = localPosition.magnitude;
			if (distanceToLocalPosition > furthestDistanceToLocalPosition)
			{
				furthestDistanceToLocalPosition = distanceToLocalPosition;
				furthestLocalPosition = localPosition;
				keyFrameIndex = i;
			}
		}
		result = new Result();
		result.furthestDistanceToLocalPosition = furthestDistanceToLocalPosition;
		result.keyFrameIndex = keyFrameIndex;
	}

	[Serializable]
	public class Result
	{
		public float furthestDistanceToLocalPosition;
		public int keyFrameIndex;
	}
}

[CustomEditor(typeof(DisplayFurthestLocalPositionInAnimation))]
public class DisplayFurthestLocalPositionInAnimationEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class DisplayFurthestLocalPositionInAnimation : EditorScript
{
}
#endif