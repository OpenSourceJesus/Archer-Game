using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
using ArcherGame;
using Random = UnityEngine.Random;

[Serializable]
public class ColorGradient2D
{
	public ColorBoundary[] colorBoundaries = new ColorBoundary[0];
	public float distanceMultiplier;

	public virtual Color Evaluate (Vector2 position)
	{
		Color output = ColorExtensions.HALF;
		foreach (ColorBoundary colorBoundary in colorBoundaries)
		{
			float lerpAmount = 1f / (colorBoundary.GetDistance(position) * colorBoundary.strength) * distanceMultiplier * Mathf.PerlinNoise(position.x, position.y);
			output = Color.Lerp(output, colorBoundary.color, lerpAmount);
		}
		return output;
	}

	public virtual Sprite MakeSprite (Vector2Int textureSize, float pixelsPerUnit)
	{
		Texture2D texture = (Texture2D) GameManager.Clone(Texture2D.whiteTexture);
		texture.Resize(textureSize.x, textureSize.y);
		for (int x = 0; x < textureSize.x; x ++)
		{
			for (int y = 0; y < textureSize.y; y ++)
				texture.SetPixel(x, y, Evaluate(new Vector2(x, y).Divide(new Vector2(textureSize.x, textureSize.y))));
		}
		texture.Apply();
		return Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize.x, textureSize.y), Vector2.one / 2, pixelsPerUnit);
	}

	public virtual Sprite MakeSprite (Vector2Int textureSize, float pixelsPerUnit, List<Color> colorsToBlendTo, List<Vector2Int> pixelsToBlendFrom)
	{
		Texture2D texture = (Texture2D) GameManager.Clone(Texture2D.whiteTexture);
		texture.Resize(textureSize.x, textureSize.y);
		for (int x = 0; x < textureSize.x; x ++)
		{
			for (int y = 0; y < textureSize.y; y ++)
			{
				Vector2Int pixel = new Vector2Int(x, y);
				Color color;
				int indexOfPixel = pixelsToBlendFrom.IndexOf(pixel);
				if (indexOfPixel != -1)
				{
					List<Color> colors = new List<Color>();
					do
					{
						colors.Add(colorsToBlendTo[indexOfPixel]);
						colorsToBlendTo.RemoveAt(indexOfPixel);
						pixelsToBlendFrom.RemoveAt(indexOfPixel);
						indexOfPixel = pixelsToBlendFrom.IndexOf(pixel);
					} while (indexOfPixel != -1);
					color = ColorExtensions.GetAverage(colors.ToArray());
				}
				else
					color = Evaluate(((Vector2) pixel).Divide((Vector2) textureSize));
				texture.SetPixel(x, y, color);
			}
		}
		texture.Apply();
		return Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize.x, textureSize.y), Vector2.one / 2, pixelsPerUnit);
	}

	public static ColorGradient2D GenerateRandom (int colorBoundaryCount, float distanceMultiplier, FloatRange strengthRange, FloatRange lengthRange, float chanceOfUsingPreviousStrength = 0, float chanceOfUsingPreviousLength = 0, float chanceOfUsingPreviousColor = 0, float chanceOfUsingPreviousPosition = 0, float chanceOfUsingPreviousRotation = 0)
	{
		ColorGradient2D output = new ColorGradient2D();
		output.distanceMultiplier = distanceMultiplier;
		float previousStrength = Random.Range(strengthRange.min, strengthRange.max);
		float previousLength = Random.Range(lengthRange.min, lengthRange.max);
		Color previousColor = ColorExtensions.RandomColor().SetAlpha(Random.value);
		Vector2 previousPosition = new Vector2(Random.value, Random.value);
		Vector2 previousDirection = Random.insideUnitCircle.normalized * previousLength / 2;
		for (int i = 0; i < colorBoundaryCount; i ++)
		{
			ColorBoundary colorBoundary = new ColorBoundary();
			float strength;
			if (Random.value < chanceOfUsingPreviousStrength)
				strength = previousStrength;
			else
			{
				strength = Random.Range(strengthRange.min, strengthRange.max);
				previousStrength = strength;
			}
			colorBoundary.strength = strength;
			float length;
			if (Random.value < chanceOfUsingPreviousLength)
				length = previousLength;
			else
			{
				length = Random.Range(lengthRange.min, lengthRange.max);
				previousLength = length;
			}
			Color color;
			if (Random.value < chanceOfUsingPreviousColor)
				color = previousColor;
			else
			{
				color = ColorExtensions.RandomColor().SetAlpha(Random.value);
				previousColor = color;
			}
			colorBoundary.color = color;
			Vector2 position;
			if (Random.value < chanceOfUsingPreviousPosition)
				position = previousPosition;
			else
			{
				position = new Vector2(Random.value, Random.value);
				previousPosition = position;
			}
			Vector2 direction;
			if (Random.value < chanceOfUsingPreviousRotation)
				direction = previousDirection;
			else
			{
				direction = Random.insideUnitCircle.normalized * length / 2;
				previousDirection = direction;
			}
			colorBoundary.lineSegment = new LineSegment2D(position + direction, position - direction);
			output.colorBoundaries = output.colorBoundaries.Add(colorBoundary);
		}
		return output;
	}

	[Serializable]
	public class ColorBoundary
	{
		public LineSegment2D lineSegment;
		public Color color;
		public float strength;

		public virtual float GetDistance (Vector2 position)
		{
			return Vector2.Distance(position, lineSegment.ClosestPoint(position));
		}
	}
}
