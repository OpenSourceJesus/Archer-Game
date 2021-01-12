#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;

public class SplitObjectOnGrid : PreBuildScript
{
	public float gridSize = 1;
	public Transform trs;

	public override void OnEnable ()
	{
		base.OnEnable ();
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			return;
		}
#endif
	}

	public override void Do ()
	{
		Rect rect = GetObjectRect();
		GameObject clone;
		PreBuildScript[] preBuildScripts;
		for (float x = rect.xMin; x < rect.xMax; x += gridSize)
		{
			for (float y = rect.yMin; y < rect.yMax; y += gridSize)
			{
				clone = Instantiate(gameObject, new Vector2(x, y) + (Vector2.one * gridSize / 2), trs.rotation);
				clone.GetComponent<Transform>().localScale = Vector3.one * gridSize;
				preBuildScripts = clone.GetComponentsInChildren<PreBuildScript>();
				for (int i = 0; i < preBuildScripts.Length; i ++)
					DestroyImmediate(preBuildScripts[i]);
			}
		}
		DestroyImmediate(gameObject);
	}

	public virtual Rect GetObjectRect ()
	{
		return Rect.MinMaxRect(trs.position.x - trs.lossyScale.x / 2, trs.position.y - trs.lossyScale.y / 2, trs.position.x + trs.lossyScale.x / 2, trs.position.y + trs.lossyScale.y / 2);
	}
}

[CustomEditor(typeof(SplitObjectOnGrid))]
public class SplitObjectOnGridEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		EditorScript editorScript = (EditorScript) target;
		editorScript.UpdateHotkeys ();
	}
}
#endif