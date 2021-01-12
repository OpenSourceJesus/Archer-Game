#if UNITY_EDITOR
using UnityEngine;
using ArcherGame;
using UnityEditor;

//[ExecuteInEditMode]
public class TeleportPlayerToMouse : EditorScript
{
    public virtual void Do ()
    {
        Player.instance.trs.position = GetMousePositionInWorld();
    }
}

[CustomEditor(typeof(TeleportPlayerToMouse))]
public class TeleportPlayerToMouseEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class TeleportPlayerToMouse : EditorScript
{
}
#endif