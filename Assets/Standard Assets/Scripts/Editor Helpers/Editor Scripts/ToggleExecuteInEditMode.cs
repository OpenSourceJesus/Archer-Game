#if UNITY_EDITOR
using UnityEngine;
using System;

public class ToggleExecuteInEditMode : EditorScript
{
	public ScriptEntry[] scriptEntries = new ScriptEntry[0];

	public override void DoEditorUpdate ()
	{

	}

	[Serializable]
	public class ScriptEntry
	{
		public MonoBehaviour script;
	}
}
#else
using UnityEngine;

public class ToggleExecuteInEditMode : EditorScript
{
}
#endif