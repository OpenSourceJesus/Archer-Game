using UnityEngine;
using System.Collections.Generic;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	[RequireComponent(typeof(SpriteRenderer))]
	public class ColorGradient2DRenderer : MonoBehaviour
	{
		public ColorGradient2D colorGradient;
		public SpriteRenderer spriteRenderer;
		public Vector2Int textureSize;
		public float pixelsPerUnit;
		public int blendRadius;
		public float blendAmount;
		public Transform trs;
		public ColorGradient2DRenderer[] blendColorGradientRenderers = new ColorGradient2DRenderer[0];
		public int colorBoundaryCount;
		public float distanceMultiplier;
		public FloatRange strengthRange;
		public FloatRange lengthRange;
		[Range(0, 1)]
		public float chanceOfUsingPreviousStrength;
		[Range(0, 1)]
		public float chanceOfUsingPreviousLength;
		[Range(0, 1)]
		public float chanceOfUsingPreviousColor;
		[Range(0, 1)]
		public float chanceOfUsingPreviousPosition;
		[Range(0, 1)]
		public float chanceOfUsingPreviousRotation;

#if UNITY_EDITOR
		public bool generateRandom;
		public bool makeSprite;
		public bool makeBlendedSprite;
		public bool makeGrayscale;
		public virtual void Update ()
		{
			if (Application.isPlaying)
				return;
			if (generateRandom)
			{
				generateRandom = false;
				GenerateRandom ();
			}
			if (makeSprite)
			{
				makeSprite = false;
				MakeSprite ();
			}
			if (makeBlendedSprite)
			{
				makeBlendedSprite = false;
				MakeBlendedSprite ();
			}
			if (makeGrayscale)
			{
				makeGrayscale = false;
				MakeGrayscale ();
			}
		}
#endif

		public virtual void GenerateRandom ()
		{
			colorGradient = ColorGradient2D.GenerateRandom(colorBoundaryCount, distanceMultiplier, strengthRange, lengthRange, chanceOfUsingPreviousStrength, chanceOfUsingPreviousLength, chanceOfUsingPreviousColor, chanceOfUsingPreviousPosition, chanceOfUsingPreviousRotation);
		}

		public virtual void MakeSprite ()
		{
			spriteRenderer.sprite = colorGradient.MakeSprite(textureSize, pixelsPerUnit);
		}

		public virtual void MakeBlendedSprite ()
		{
			List<Vector2Int> pixelsToBlendFrom = new List<Vector2Int>();
			List<Color> colorsToBlendTo = new List<Color>();
			foreach (ColorGradient2DRenderer blendToColorGradientRenderer in blendColorGradientRenderers)
			{
				if (blendToColorGradientRenderer.trs.position.x > trs.position.x)
				{
					for (int y = 0; y < textureSize.y; y ++)
					{
						for (int x = 0; x < blendRadius; x ++)
						{
							Vector2Int pixelToBlendFrom = new Vector2Int(textureSize.x - x, y);
							int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
							Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(0, y);
							if (indexOfPixelToBlendFrom != -1)
							{
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], x - x * blendAmount);
								colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
								pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
							}
							else
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorGradient.Evaluate(((Vector2) pixelToBlendFrom).Divide((Vector2) textureSize)), x - x * blendAmount);
							pixelsToBlendFrom.Add(pixelToBlendFrom);
							colorsToBlendTo.Add(colorToBlendTo);
						}
					}
				}
				else if (blendToColorGradientRenderer.trs.position.x < trs.position.x)
				{
					for (int y = 0; y < textureSize.y; y ++)
					{
						for (int x = 0; x < blendRadius; x ++)
						{
							Vector2Int pixelToBlendFrom = new Vector2Int(x, y);
							int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
							Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(textureSize.x - 1, y);
							if (indexOfPixelToBlendFrom != -1)
							{
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], x - x * blendAmount);
								colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
								pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
							}
							else
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorGradient.Evaluate(((Vector2) pixelToBlendFrom).Divide((Vector2) textureSize)), x - x * blendAmount);
							pixelsToBlendFrom.Add(pixelToBlendFrom);
							colorsToBlendTo.Add(colorToBlendTo);
						}
					}
				}
				if (blendToColorGradientRenderer.trs.position.y > trs.position.y)
				{
					for (int x = 0; x < textureSize.x; x ++)
					{
						for (int y = 0; y < blendRadius; y ++)
						{
							Vector2Int pixelToBlendFrom = new Vector2Int(x, textureSize.y - y);
							int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
							Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(x, 0);
							if (indexOfPixelToBlendFrom != -1)
							{
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], y - y * blendAmount);
								colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
								pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
							}
							else
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorGradient.Evaluate(((Vector2) pixelToBlendFrom).Divide((Vector2) textureSize)), y - y * blendAmount);
							pixelsToBlendFrom.Add(pixelToBlendFrom);
							colorsToBlendTo.Add(colorToBlendTo);
						}
					}
				}
				else if (blendToColorGradientRenderer.trs.position.y < trs.position.y)
				{
					for (int x = 0; x < textureSize.x; x ++)
					{
						for (int y = 0; y < blendRadius; y ++)
						{
							Vector2Int pixelToBlendFrom = new Vector2Int(x, y);
							int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
							Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(x, textureSize.y - 1);
							if (indexOfPixelToBlendFrom != -1)
							{
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], y - y * blendAmount);
								colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
								pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
							}
							else
								colorToBlendTo = Color.Lerp(colorToBlendTo, colorGradient.Evaluate(((Vector2) pixelToBlendFrom).Divide((Vector2) textureSize)), y - y * blendAmount);
							pixelsToBlendFrom.Add(pixelToBlendFrom);
							colorsToBlendTo.Add(colorToBlendTo);
						}
					}
				}
			}
			spriteRenderer.sprite = colorGradient.MakeSprite(textureSize, pixelsPerUnit, colorsToBlendTo, pixelsToBlendFrom);
		}

		public virtual void MakeGrayscale ()
		{
			Texture2D texture = spriteRenderer.sprite.texture;
			Color[] colors = texture.GetPixels();
			for (int x = 0; x < texture.width; x ++)
			{
				for (int y = 0; y < texture.height; y ++)
				{
					Color color = colors[x + y * texture.width];
					color = ((Color) ColorExtensions.WHITE).Multiply(color.GetBrightness()).SetAlpha(color.a);
					colors[x + y * texture.width] = color;
				}
			}
			texture.SetPixels(colors);
			texture.Apply ();
		}
	}
}