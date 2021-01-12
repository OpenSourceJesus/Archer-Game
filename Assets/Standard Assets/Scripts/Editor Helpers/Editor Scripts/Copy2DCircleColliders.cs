/*
To Do:
1) Make script work attached to a transform with any values
*/

#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;
using UnityEditor;

//[ExecuteInEditMode]
public class Copy2DCircleColliders : EditorScript
{
	public CircleCollider2D[] oldCircleColliders;
	public CircleCollider2D[] newCircleColliders;

	public virtual void Do ()
	{
		for (int i = 0; i < newCircleColliders.Length; i ++)
			DestroyImmediate(newCircleColliders[i]);
		newCircleColliders = new CircleCollider2D[0];
		CircleCollider2D newCircleCollider;
		Transform oldCircleColliderTrs;
		foreach (CircleCollider2D oldCircleCollider in oldCircleColliders)
		{
			oldCircleColliderTrs = oldCircleCollider.GetComponent<Transform>();
			newCircleCollider = gameObject.AddComponent<CircleCollider2D>();
			newCircleCollider.offset = (Vector2) oldCircleColliderTrs.position + oldCircleCollider.offset.Multiply(oldCircleColliderTrs.lossyScale);
			newCircleCollider.radius = oldCircleCollider.radius * Mathf.Abs(oldCircleColliderTrs.lossyScale.x);
			newCircleCollider.isTrigger = oldCircleCollider.isTrigger;
			newCircleCollider.enabled = oldCircleCollider.enabled;
			newCircleColliders = newCircleColliders.Add(newCircleCollider);
		}
	}
}

[CustomEditor(typeof(Copy2DCircleColliders))]
public class Copy2DCircleCollidersEditor : EditorScriptEditor
{
}
#endif