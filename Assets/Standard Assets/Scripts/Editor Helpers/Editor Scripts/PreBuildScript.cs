#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;

public class PreBuildScript : EditorScript
{
    public virtual void Do ()
    {
    }
}

[CustomEditor(typeof(PreBuildScript))]
public class PreBuildScriptEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class PreBuildScript : EditorScript
{
}
#endif