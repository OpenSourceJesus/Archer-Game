using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
using ArcherGame;
using Random = UnityEngine.Random;

[Serializable]
public class SampledColorGradient2D
{
	public Vector2Int size;
	public ColorArea[] colorAreas = new ColorArea[0];
	public Color[] colors = new Color[0];
	public int blurRadius;
	public int borderSize;

	public virtual Color[] GetColors ()
	{
		Color[] output = new Color[size.x * size.y];
		for (int x = 0; x < size.x; x ++)
		{
			for (int y = 0; y < size.y; y ++)
				output[x + y * size.x] = GetColor(new Vector2Int(x, y));
		}
		return output;
	}

	public virtual Color GetColor (Vector2Int position)
	{
		List<Color> colors = new List<Color>();
		for (int x = Mathf.Clamp(position.x - blurRadius, 0, size.x); x <= Mathf.Clamp(position.x + blurRadius, 0, size.x - 1); x ++)
		{
			for (int y = Mathf.Clamp(position.y - blurRadius, 0, size.y); y <= Mathf.Clamp(position.y + blurRadius, 0, size.y - 1); y ++)
			{
				if (new Vector2Int(x, y) != position)
					colors.Add(this.colors[x + y * size.x]);
			}
		}
		return ColorExtensions.GetAverage(colors.ToArray());
	}

	public virtual Sprite MakeSprite (Vector2Int textureSize, float pixelsPerUnit)
	{
		Texture2D texture = (Texture2D) GameManager.Clone(Texture2D.whiteTexture);
		texture.Resize(textureSize.x, textureSize.y);
		texture.SetPixels(GetColors());
		texture.Apply();
		return Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize.x, textureSize.y), Vector2.one / 2, pixelsPerUnit);
	}

	public virtual Sprite MakeSprite (Vector2Int textureSize, float pixelsPerUnit, List<Color> colorsToBlendTo, List<Vector2Int> pixelsToBlendFrom)
	{
		Texture2D texture = (Texture2D) GameManager.Clone(Texture2D.whiteTexture);
		texture.Resize(textureSize.x + borderSize * 2, textureSize.y + borderSize * 2);
		Color[] _colors = new Color[texture.width * texture.height];
		for (int i = 0; i < _colors.Length; i ++)
			_colors[i] = ColorExtensions.CLEAR;
		texture.SetPixels(_colors);
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
					color = GetColor(new Vector2Int(x, y));
				texture.SetPixel(x + borderSize, y + borderSize, color);
			}
		}
		texture.Apply();
		return Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize.x + borderSize * 2, textureSize.y + borderSize * 2), Vector2.one / 2, pixelsPerUnit);
	}

	public static SampledColorGradient2D GenerateRandom (Vector2Int size, int colorAreaCount, int blurRadius, int borderSize)
	{
		SampledColorGradient2D output = new SampledColorGradient2D();
		output.blurRadius = blurRadius;
		output.size = size;
		output.borderSize = borderSize;
		output.colors = new Color[size.x * size.y];
		List<Vector2Int> remainingPositions = new List<Vector2Int>();
		for (int x = 0; x < size.x; x ++)
		{
			for (int y = 0; y < size.y; y ++)
				remainingPositions.Add(new Vector2Int(x, y));
		}
		for (int i = 0; i < colorAreaCount; i ++)
		{
			ColorArea colorArea = new ColorArea();
			colorArea.origin = new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
			colorArea.edgePositions.Add(colorArea.origin);
			colorArea.color = ColorExtensions.RandomColor();//.SetAlpha(Random.value);
			output.colorAreas = output.colorAreas.Add(colorArea);
			remainingPositions.Remove(colorArea.origin);
			output.colors[colorArea.origin.x + colorArea.origin.y * size.x] = colorArea.color;
		}
		while (remainingPositions.Count > 0)
		{
			for (int i = 0; i < colorAreaCount; i ++)
			{
				ColorArea colorArea = output.colorAreas[i];
				int edgePositionCount = colorArea.edgePositions.Count;
				if (edgePositionCount > 0)
				{
					for (int i2 = 0; i2 < edgePositionCount; i2 ++)
					{
						Vector2Int edgePosition = colorArea.edgePositions[i2];
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.right, ref remainingPositions);
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.left, ref remainingPositions);
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.up, ref remainingPositions);
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.down, ref remainingPositions);
						colorArea.edgePositions.Remove(edgePosition);
						edgePositionCount --;
					}
					output.colorAreas[i] = colorArea;
				}
			}
		}
		return output;
	}

	public static IEnumerator GenerateRandomRoutine (Vector2Int size, int colorAreaCount, int blurRadius, int borderSize)
	{
		SampledColorGradient2D output = new SampledColorGradient2D();
		output.blurRadius = blurRadius;
		output.size = size;
		output.borderSize = borderSize;
		output.colors = new Color[size.x * size.y];
		List<Vector2Int> remainingPositions = new List<Vector2Int>();
		for (int x = 0; x < size.x; x ++)
		{
			for (int y = 0; y < size.y; y ++)
				remainingPositions.Add(new Vector2Int(x, y));
		}
		for (int i = 0; i < colorAreaCount; i ++)
		{
			ColorArea colorArea = new ColorArea();
			colorArea.origin = new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
			colorArea.edgePositions.Add(colorArea.origin);
			colorArea.color = ColorExtensions.RandomColor();//.SetAlpha(Random.value);
			output.colorAreas = output.colorAreas.Add(colorArea);
			remainingPositions.Remove(colorArea.origin);
			output.colors[colorArea.origin.x + colorArea.origin.y * size.x] = colorArea.color;
		}
		while (remainingPositions.Count > 0)
		{
			for (int i = 0; i < colorAreaCount; i ++)
			{
				ColorArea colorArea = output.colorAreas[i];
				int edgePositionCount = colorArea.edgePositions.Count;
				if (edgePositionCount > 0)
				{
					for (int i2 = 0; i2 < edgePositionCount; i2 ++)
					{
						Vector2Int edgePosition = colorArea.edgePositions[i2];
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.right, ref remainingPositions);
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.left, ref remainingPositions);
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.up, ref remainingPositions);
						output.TryToSpreadColorAreaToPosition(ref colorArea, edgePosition + Vector2Int.down, ref remainingPositions);
						colorArea.edgePositions.Remove(edgePosition);
						edgePositionCount --;
					}
					output.colorAreas[i] = colorArea;
				}
			}
			yield return new WaitForEndOfFrame();
		}
		yield return output;
	}
	
	public virtual bool TryToSpreadColorAreaToPosition (ref ColorArea colorArea, Vector2Int position, ref List<Vector2Int> remainingPositions)
	{
		bool output = remainingPositions.Contains(position);
		if (output)
		{
			colorArea.edgePositions.Add(position);
			remainingPositions.Remove(position);
			colors[position.x + position.y * size.x] = colorArea.color;
		}
		return output;
	}

	[Serializable]
	public class ColorArea
	{
		public Vector2Int origin;
		public List<Vector2Int> edgePositions = new List<Vector2Int>();
		public Color color;
	}
}
