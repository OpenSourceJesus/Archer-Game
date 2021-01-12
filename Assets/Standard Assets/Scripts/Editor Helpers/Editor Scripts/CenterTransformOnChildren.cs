#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CenterTransformOnChildren : EditorScript
{
    public Transform trs;

    public virtual void Do ()
    {
        
    }
}

[CustomEditor(typeof(CenterTransformOnChildren))]
public class CenterTransformOnChildrenEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class CenterTransformOnChildren : EditorScript
{
}
#endif