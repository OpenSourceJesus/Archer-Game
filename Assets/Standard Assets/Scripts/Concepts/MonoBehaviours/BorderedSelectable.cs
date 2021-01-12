using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class BorderedSelectable : _Selectable
{
    public Selectable[] _graphics;
    public RectTransform combinedRectTrs;

    public virtual void Start ()
    {
        Canvas.ForceUpdateCanvases();
        RectTransform[] children = GetComponentsInChildren<RectTransform>();
        children = children.Remove(combinedRectTrs);
        Rect[] graphicRects = new Rect[children.Length];
        for (int i = 0; i < children.Length; i ++)
            graphicRects[i] = children[i].GetRectInCanvasNormalized(canvasRectTrs);
        Rect combinedRect = RectExtensions.Combine(graphicRects);
        Vector2 combinedRectCenter;
        combinedRectCenter = combinedRect.center;
        combinedRect.size = combinedRect.size.Multiply(canvasRectTrs.sizeDelta);
        combinedRect.center = combinedRectCenter.Multiply(canvasRectTrs.sizeDelta);
        combinedRectTrs.sizeDelta = combinedRect.size;
        rectTrs = combinedRectTrs;
    }

    public virtual void LateUpdate ()
    {
        for (int i = 0; i < _graphics.Length ; i ++)
            _graphics[i].colors = selectable.colors;
    }
}
