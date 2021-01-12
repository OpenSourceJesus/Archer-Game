#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// [ExecuteInEditMode]
public class SetAnimationClipsRoot : MonoBehaviour
{
	public AnimationClip[] animationClips = new AnimationClip[0];
	public GameObject currentRootGo;
	public Transform newRoot;

	void OnEnable ()
	{
		for (int i = 0; i < animationClips.Length; i ++)
		{
			AnimationClip animationClip = animationClips[i];
			EditorCurveBinding[] editorCurveBindings = AnimationUtility.GetCurveBindings(animationClip);
			AnimationCurve[] animationCurves = new AnimationCurve[editorCurveBindings.Length];
			for (int i2 = 0; i2 < editorCurveBindings.Length; i2 ++)
			{
				EditorCurveBinding editorCurveBinding = editorCurveBindings[i2];
				print(editorCurveBinding.path + " - " + editorCurveBinding.propertyName);
				animationCurves[i2] = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
				Object animatedObject = AnimationUtility.GetAnimatedObject(currentRootGo, editorCurveBinding);
				Transform animatedObjectTrs = animatedObject as Transform;
				if (animatedObjectTrs != null)
					editorCurveBinding.path = AnimationUtility.CalculateTransformPath(animatedObjectTrs, newRoot);
				// animationCurves[i2] = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
			}
			AnimationUtility.SetEditorCurves(animationClip, editorCurveBindings, animationCurves);
		}
	}
}
#endif