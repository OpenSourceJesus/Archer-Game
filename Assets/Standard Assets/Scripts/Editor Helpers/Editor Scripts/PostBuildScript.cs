#if UNITY_EDITOR
using UnityEngine;
using System;

public class PostBuildScript : EditorScript
{
    public virtual void Do ()
    {
    }
}
#endif
#if !UNITY_EDITOR
public class PostBuildScript : EditorScript
{
}
#endif