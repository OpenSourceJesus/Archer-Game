using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
	public static class RectTransformExtensions
	{
		public static Rect GetWorldRect (this RectTransform rectTrs)
		{
			Vector2 min = rectTrs.TransformPoint(rectTrs.rect.min);
			Vector2 max = rectTrs.TransformPoint(rectTrs.rect.max);
			return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		}
		
		public static Vector2 GetCenterInCanvasNormalized (this RectTransform rectTrs, RectTransform canvasRectTrs)
		{
			return canvasRectTrs.GetWorldRect().ToNormalizedPosition(rectTrs.GetWorldRect().center);
		}

		public static Rect GetRectInCanvasNormalized (this RectTransform rectTrs, RectTransform canvasRectTrs)
		{
			Rect output = rectTrs.GetWorldRect();
			Rect canvasRect = canvasRectTrs.GetWorldRect();
			Vector2 outputMin = canvasRect.ToNormalizedPosition(output.min);
			Vector2 outputMax = canvasRect.ToNormalizedPosition(output.max);
			return Rect.MinMaxRect(outputMin.x, outputMin.y, outputMax.x, outputMax.y);
		}

		// public static void SetAnchorsToRect (this RectTransform rectTrs)
		// {
		// 	rectTrs.SetInsetAndSizeFromParentEdge();
		// }
	}
}