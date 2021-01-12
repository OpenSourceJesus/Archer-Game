#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ArcherGame;

public class ReplaceWithPrefabs : EditorScript
{
	public Transform[] replace;
	public Transform prefab;
	
	public virtual void Do ()
	{
		foreach (Transform trs in replace)
		{
			Transform clone = (Transform) PrefabUtility.InstantiatePrefab(prefab);
			clone.position = trs.position;
			clone.rotation = trs.rotation;
			clone.SetParent(trs.parent);
			clone.localScale = trs.localScale;
			DestroyImmediate(trs.gameObject);
		}
	}
}

[CustomEditor(typeof(ReplaceWithPrefabs))]
public class ReplaceWithPrefabsEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		EditorScript editorScript = (EditorScript) target;
		editorScript.UpdateHotkeys ();
	}
}
#endif