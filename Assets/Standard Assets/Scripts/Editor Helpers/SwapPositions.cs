#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Extensions;

[ExecuteInEditMode]
public class SwapPositions : MonoBehaviour
{
	public static float lastSwapTime;
	public float minSwapTimeInterval = .1f;

	public virtual void Start ()
	{
		if (Time.unscaledTime - lastSwapTime < minSwapTimeInterval)
			return;
		lastSwapTime = Time.unscaledTime;
		Transform[] transforms = new Transform[0];
		transforms = transforms.Add(Selection.transforms[0]);
		transforms = transforms.Add(Selection.transforms[1]);
		Vector3 previousPosition0 = transforms[0].position;
		transforms[0].position = transforms[1].position;
		transforms[1].position = previousPosition0;
		DestroyImmediate(Selection.transforms[0].GetComponent<SwapPositions>());
		DestroyImmediate(Selection.transforms[0].GetComponent<SwapPositions>());
	}
}
#endif