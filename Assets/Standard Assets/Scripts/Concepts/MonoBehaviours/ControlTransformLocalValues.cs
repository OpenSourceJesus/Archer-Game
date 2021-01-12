using UnityEngine;

[ExecuteInEditMode]
public class ControlTransformLocalValues : MonoBehaviour
{
	public Vector3 localPosition;
	public Vector3 localRotation;
	public Vector3 localScale;
	public Transform trs;

#if UNITY_EDITOR
	void Start ()
	{
		trs = GetComponent<Transform>();
	}
#endif
}