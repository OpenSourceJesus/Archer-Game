using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Extensions
{
	public static class RectExtensions
	{
		public static Rect NULL = new Rect(MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT);

		public static Rect Move (this Rect rect, Vector2 movement)
		{
			rect.position += movement;
			return rect;
		}
		
		public static RectInt Move (this RectInt rect, Vector2Int movement)
		{
			rect.position += movement;
			return rect;
		}

		public static Rect SwapXAndY (this Rect rect)
		{
			return Rect.MinMaxRect(rect.min.y, rect.min.x, rect.max.y, rect.max.x);
		}
		
		public static bool IsEncapsulating (this Rect r1, Rect r2, bool equalRectsRetunsTrue)
		{
			if (equalRectsRetunsTrue)
			{
				bool minIsOk = r1.min.x <= r2.min.x && r1.min.y <= r2.min.y;
				bool maxIsOk = r1.max.x >= r2.max.x && r1.max.y >= r2.max.y;
				return minIsOk && maxIsOk;
			}
			else
			{
				bool minIsOk = r1.min.x < r2.min.x && r1.min.y < r2.min.y;
				bool maxIsOk = r1.max.x > r2.max.x && r1.max.y > r2.max.y;
				return minIsOk && maxIsOk;
			}
		}
		
		public static bool IsIntersecting (this Rect r1, Rect r2, bool equalRectsRetunsTrue = true)
		{
			if (equalRectsRetunsTrue)
				return r1.xMin <= r2.xMax && r1.xMax >= r2.xMin && r1.yMin <= r2.yMax && r1.yMax >= r2.yMin;
			else
				return r1.xMin < r2.xMax && r1.xMax > r2.xMin && r1.yMin < r2.yMax && r1.yMax > r2.yMin;
		}

		public static Vector2[] GetCorners (this Rect rect)
		{
			Vector2[] output = new Vector2[4];
			output[0] = rect.min;
			output[1] = new Vector2(rect.xMax, rect.yMin);
			output[2] = new Vector2(rect.xMin, rect.yMax);
			output[3] = rect.max;
			return output;
		}
		
		public static bool IsExtendingOutside (this Rect r1, Rect r2, bool equalRectsRetunsTrue)
		{
			if (equalRectsRetunsTrue)
			{
				bool minIsOk = r1.min.x <= r2.min.x || r1.min.y <= r2.min.y;
				bool maxIsOk = r1.max.x >= r2.max.x || r1.max.y >= r2.max.y;
				return minIsOk || maxIsOk;
			}
			else
			{
				bool minIsOk = r1.min.x < r2.min.x || r1.min.y < r2.min.y;
				bool maxIsOk = r1.max.x > r2.max.x || r1.max.y > r2.max.y;
				return minIsOk || maxIsOk;
			}
		}
		
		public static Rect ToRect (this Bounds bounds)
		{
			return Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
		}

		public static Rect Combine (params Rect[] rects)
		{
			Rect output = rects[0];
			for (int i = 1; i < rects.Length; i ++)
			{
				Rect rect = rects[i];
				if (rect.xMin < output.xMin)
					output.xMin = rect.xMin;
				if (rect.xMax > output.xMax)
					output.xMax = rect.xMax;
				if (rect.yMin < output.yMin)
					output.yMin = rect.yMin;
				if (rect.yMax > output.yMax)
					output.yMax = rect.yMax;
			}
			return output;
		}

		public static Rect FromPoints (params Vector2[] points)
		{
			Vector2 point = points[0];
			Rect output = Rect.MinMaxRect(point.x, point.y, point.x, point.y);
			for (int i = 1; i < points.Length; i ++)
			{
				point = points[i];
				if (point.x < output.min.x)
					output.min = new Vector2(point.x, output.min.y);
				if (point.y < output.min.y)
					output.min = new Vector2(output.min.x, point.y);
				if (point.x > output.max.x)
					output.max = new Vector2(point.x, output.max.y);
				if (point.y > output.max.y)
					output.max = new Vector2(output.max.x, point.y);
			}
			return output;
		}

		public static Rect Expand (this Rect rect, Vector2 amount)
		{
			Vector2 center = rect.center;
			rect.size += amount;
			rect.center = center;
			return rect;
		}
		
		public static Rect Set (this Rect rect, RectInt rectInt)
		{
			rect.center = rectInt.center;
			rect.size = rectInt.size;
			return rect;
		}

		public static Rect ToRect (this RectInt rectInt)
		{
			return Rect.MinMaxRect(rectInt.xMin, rectInt.yMin, rectInt.xMax, rectInt.yMax);
		}

		public static Vector2 ClosestPoint (this Rect rect, Vector2 point)
		{
			return point.ClampComponents(rect.min, rect.max);
		}

		public static Vector2 ToNormalizedPosition (this Rect rect, Vector2 point)
		{
			return Rect.PointToNormalized(rect, point);
			// return Vector2.one.Divide(rect.size) * (point - rect.min);
		}

		public static Vector2 ToNormalizedPosition (this RectInt rect, Vector2Int point)
		{
			return Vector2.one.Divide(rect.size.ToVec2()).Multiply(point.ToVec2() - rect.min.ToVec2());
		}

		public static Rect SetPositiveSize (this Rect rect)
		{
			return rect.SetSize(new Vector2(Mathf.Abs(rect.size.x),  Mathf.Abs(rect.size.y)));
		}

		public static Circle2D GetSmallestCircleAround (this Rect rect)
		{
			return new Circle2D(rect.center, rect.size.magnitude / 2);
		}

		// public static Rect GetExactFitRectForCircle (Circle2D circle)
		// {
		// }

		public static Rect AnchorToPoint (this Rect rect, Vector2 point, Vector2 anchorPoint)
		{
			Rect output = rect;
			output.position = point - (output.size * anchorPoint);
			return output;
		}

		public static LineSegment2D[] GetEdges (this Rect rect)
		{
			return new LineSegment2D[4] { new LineSegment2D(rect.min, new Vector2(rect.xMin, rect.yMax)), new LineSegment2D(rect.min, new Vector2(rect.xMax, rect.yMin)), new LineSegment2D(rect.max, new Vector2(rect.xMin, rect.yMax)), new LineSegment2D(rect.max, new Vector2(rect.xMax, rect.yMin)) };
		}

		public static Bounds ToBounds (this Rect rect)
		{
			return new Bounds(rect.center, rect.size);
		}

		public static Bounds ToBounds (this RectInt rect)
		{
			return new Bounds(rect.center, rect.size.ToVec2());
		}

		public static BoundsInt ToBoundsInt (this RectInt rect)
		{
			return new BoundsInt(rect.center.ToVec3Int(), rect.size.ToVec3Int());
		}

		public static Rect GrowToPoint (this Rect rect, Vector2 point)
		{
			rect.min = rect.min.SetToMinComponents(point);
			rect.max = rect.max.SetToMaxComponents(point);
			return rect;
		}

		public static Rect SetSize (this Rect rect, Vector2 size)
		{
			Rect output = rect;
			output.size = new Vector2(size.x, size.y);
			output.center = rect.center;
			return output;
		}

		public static Rect MultiplySize (this Rect rect, Vector2 multiplySize)
		{
			Rect output = rect;
			output.size = output.size.Multiply(multiplySize);
			output.center = rect.center;
			return output;
		}

		public static Rect MultiplySize (this Rect rect, float edgeLength)
		{
			Rect output = rect;
			output.size *= edgeLength;
			output.center = rect.center;
			return output;
		}

		public static Vector2 RandomPointOnPerimeter (this Rect rect)
		{
			LineSegment2D[] edges = new LineSegment2D[4];
			float[] edgeLengths = new float[4];
			float perimeterLength = 0;
			for (int i = 0; i < 4; i ++)
			{
				edgeLengths[i] = edges[i].GetLength();
				perimeterLength += edgeLengths[i];
			}
			float distance = Random.value * perimeterLength;
			LineSegment2D previousEdge = edges[0];
			for (int i = 1; i < 4; i ++)
			{
				float edgeLength = edgeLengths[i];
				if (distance > edgeLength)
					distance -= edgeLength;
				else
					return edges[i].GetPointWithDirectedDistance(distance);
			}
			throw new Exception("How did this exception run?");
		}
		
		public static Vector3 RandomPoint (this Rect rect)
		{
			return new Vector3(Random.Range(rect.min.x, rect.max.x), Random.Range(rect.min.y, rect.max.y));
		}
	}
}