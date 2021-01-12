using UnityEngine;
using System;
using Extensions;

[Serializable]
public class NormalizedRect
{
    public Vector2 center;
    public Vector2 size;
    public Vector2 min;
    public Vector2 max;

    public virtual void UpdateMinMax ()
    {
        min = center - size / 2;
        max = center + size / 2;
    }
    
    public virtual void UpdateSize ()
    {
        size = max - min;
    }

    public virtual void UpdateCenter ()
    {
        center = (min + max) / 2;
    }

    public virtual Rect Apply (Rect rect)
    {
        Rect output = rect;
        output.size *= size;
        output.center *= center * 2;
        return output;
    }

    public virtual RectInt Apply (RectInt rect)
    {
        RectInt output = rect;
        output.min = rect.min + rect.size.Multiply(min);
        output.max = rect.max.Multiply(max);
        return output;
    }
}
