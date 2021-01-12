using UnityEngine;
using Extensions;
using System;

[Serializable]
public class ColorTransparencyFilter : ImageFilter
{
	public Color color;
	public AnimationCurve transparencyOverSimilarityCurve;
	public float accuracy;
	public bool shouldTestAlpha;

	public override void Apply ()
	{
		Color[] pixelColors = image.GetPixels();
		for (int x = 0; x < image.width; x ++)
		{
			for (int y = 0; y < image.height; y ++)
			{
				Apply (new Vector2Int(x, y), ref pixelColors);
			}
		}
		image.SetPixels(pixelColors);
		image.Apply();
	}

	public override void Apply (Vector2Int pixelPosition, ref Color[] pixelColors)
	{
		Color pixelColor = pixelColors[pixelPosition.x + pixelPosition.y * image.width];
		float colorSimilarity = pixelColor.GetSimilarity(color, shouldTestAlpha);
		pixelColors[pixelPosition.x + pixelPosition.y * image.width].a = Mathf.Lerp(pixelColor.a, transparencyOverSimilarityCurve.Evaluate(colorSimilarity), applyAmount);
	}
}