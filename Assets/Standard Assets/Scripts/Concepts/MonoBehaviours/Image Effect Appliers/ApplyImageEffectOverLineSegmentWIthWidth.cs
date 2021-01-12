using UnityEngine;
using Extensions;

public class ApplyImageEffectOverLineSegmentWithWidth<TImageEffect> : ImageEffectApplier<TImageEffect> where TImageEffect : ImageEffect
{
	public LineSegment2D lineSegment;
	public float width;
	public float applyAmount;
	public float distanceBetweenPixels;
	public float checksPerDistanceForStartPoint;
	public float checksPerDistanceForWidth;

	public override void OnEnable ()
	{
		SetApplyAmounts ();
		base.OnEnable ();
	}

#if UNITY_EDITOR
	void OnValidate ()
	{
		SetApplyAmounts ();
	}
#endif

	public override bool SetApplyAmounts ()
	{
		bool output = false;
		LineSegment2D perpendicularLineSegment = lineSegment.GetPerpendicular();
		for (float distance = -width; distance <= width; distance += checksPerDistanceForWidth)
		{
			LineSegment2D _lineSegment = lineSegment.Move(perpendicularLineSegment.GetPointWithDirectedDistance(distance) - lineSegment.GetMidpoint());
			output |= SetApplyAmounts (_lineSegment, checksPerDistanceForStartPoint);
		}
		return output;
	}

	public virtual bool SetApplyAmounts (LineSegment2D lineSegment, float checksPerDistanceForStartPoint)
	{
		float length = lineSegment.GetLength();
		Texture2D image = imageEffect.image;
		for (int x = 0; x < image.width; x ++)
		{
			for (int y = 0; y < image.height; y ++)
				applyAmounts[x + y * image.width] = 0;
		}
		RectInt imageRect = new RectInt(0, 0, image.width, image.height);
		Vector2 previousPixelPosition = lineSegment.start.ToVec2Int();
		for (float distance = 0; distance <= length; distance += 1f / checksPerDistanceForStartPoint)
		{
			Vector2 pixelPosition = lineSegment.GetPointWithDirectedDistance(distance);
			if (imageRect.Contains(pixelPosition.ToVec2Int()))
			{
				pixelPosition = previousPixelPosition.ClampComponents(imageRect.min, imageRect.max);
				applyAmounts[Mathf.RoundToInt(pixelPosition.x) + Mathf.RoundToInt(pixelPosition.y) * imageRect.width] = applyAmount;
				float slope = lineSegment.GetSlope();
				float xDelta;
				float yDelta;
				if (Mathf.Abs(slope) <= 1)
				{
					xDelta = Mathf.Sign(lineSegment.GetDirection().x);
					yDelta = Mathf.Sign(lineSegment.GetDirection().y) * Mathf.Abs(slope);
				}
				else
				{
					xDelta = Mathf.Sign(lineSegment.GetDirection().x) / Mathf.Abs(slope);
					yDelta = Mathf.Sign(lineSegment.GetDirection().y);
				}
				Vector2 deltaVector = new Vector2(xDelta, yDelta);
				float deltaDistance = distanceBetweenPixels * deltaVector.magnitude;
				while (distance < length)
				{
					pixelPosition += deltaVector;
					if (!imageRect.Contains(pixelPosition.ToVec2Int()))
						return true;
					applyAmounts[Mathf.RoundToInt(pixelPosition.x) + Mathf.RoundToInt(pixelPosition.y) * imageRect.width] = applyAmount;
					distance += deltaDistance;
				}
				return true;
			}
			previousPixelPosition = pixelPosition;
		}
		return false;
	}
}