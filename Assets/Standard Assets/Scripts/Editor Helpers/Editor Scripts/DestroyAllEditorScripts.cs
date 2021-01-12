#if UNITY_EDITOR
using UnityEngine;
using Extensions;

public class DestroyAllEditorScripts : EditorScript
{
	public virtual void Start ()
	{
		Do ();
	}

	public virtual void Do ()
	{
		EditorScript[] editorScripts = FindObjectsOfType<EditorScript>().Remove(this);
		foreach (EditorScript editorScript in editorScripts)
			DestroyImmediate(editorScript);
		DestroyImmediate(this);
	}
}
#endif