#if UNITY_EDITOR
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

//[ExecuteInEditMode]
public class ClearTilemaps : EditorScript
{
	// public bool update;
	public Transform tilemapsParent;
	List<Tilemap> tilemaps = new List<Tilemap>();

	// public override void Update ()
	// {
	// 	if (!update)
	// 		return;
	// 	update = false;
	// 	Do ();
	// }

	public virtual void Do ()
	{
		foreach (Tilemap tilemap in tilemapsParent.GetComponentsInChildren<Tilemap>())
			tilemap.ClearAllTiles ();
	}
}

[CustomEditor(typeof(ClearTilemaps))]
public class ClearTilemapsEditor : EditorScriptEditor
{
}
#endif