using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class _LineRenderer : MonoBehaviour, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public LineRenderer lineRenderer;
	public Transform[] points = new Transform[0];

	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (lineRenderer == null)
				lineRenderer = GetComponent<LineRenderer>();
			return;
		}
#endif
	}

	public virtual void DoUpdate ()
	{
		if (points.Length == 0)
			return;
		lineRenderer.positionCount = points.Length;
		for (int i = 0; i < points.Length; i ++)
			lineRenderer.SetPosition(i, points[i].position);
	}
}
