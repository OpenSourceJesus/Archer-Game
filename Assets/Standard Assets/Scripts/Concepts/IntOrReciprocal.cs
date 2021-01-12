#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class IntOrReciprocal
{
    public int integer;
    public bool isReciprocal;

    public virtual float GetValue ()
    {
        if (isReciprocal)
            return 1f / integer;
        else
            return integer;
    }

    public override string ToString ()
    {
        return "" + GetValue();
    }
}
#endif