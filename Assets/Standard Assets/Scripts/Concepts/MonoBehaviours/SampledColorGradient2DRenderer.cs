using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extensions;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using EditorCoroutineWithData = ThreadingUtilities.EditorCoroutineWithData;
#endif
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;

namespace ArcherGame
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SpriteRenderer))]
	public class SampledColorGradient2DRenderer : MonoBehaviour
	{
		public bool generateRandom;
		public bool update;
		public SampledColorGradient2D sampledColorGradient;
		public SpriteRenderer spriteRenderer;
		public Vector2Int textureSize;
		public float pixelsPerUnit;
		public int colorAreaCount;
		public int blurRadius;
		public int borderSize;
		public bool waitUntilDone;
		public Transform trs;
		public SampledColorGradient2DRenderer[] blendColorGradientRenderers = new SampledColorGradient2DRenderer[0];
		public float blendRange;
#if UNITY_EDITOR
		EditorCoroutine editorCoroutine;
#endif

#if UNITY_EDITOR
		void OnValidate ()
		{
			if (generateRandom)
			{
				generateRandom = false;
				GenerateRandom ();
			}
			if (update)
			{
				update = false;
				UpdateRenderer ();
			}
		}
#endif

		public void GenerateRandom ()
		{
#if UNITY_EDITOR
			if (editorCoroutine != null)
				EditorCoroutineUtility.StopCoroutine(editorCoroutine);
			editorCoroutine = EditorCoroutineUtility.StartCoroutine(GenerateRandomRoutine (), this);
#else
			StartCoroutine(GenerateRandomRoutine ());
#endif
		}

		public IEnumerator GenerateRandomRoutine ()
		{
			if (!waitUntilDone)
			{
#if UNITY_EDITOR
				EditorCoroutineWithData coroutineWithData = new EditorCoroutineWithData(this, SampledColorGradient2D.GenerateRandomRoutine(textureSize, colorAreaCount, blurRadius, borderSize));
				yield return new ThreadingUtilities.WaitForReturnedValueOfType_Editor<SampledColorGradient2D>(coroutineWithData);
#else
				CoroutineWithData coroutineWithData = new CoroutineWithData(this, SampledColorGradient2D.GenerateRandomRoutine(textureSize, colorAreaCount, blurRadius, borderSize));
				yield return new ThreadingUtilities.WaitForReturnedValueOfType<SampledColorGradient2D>(coroutineWithData);
#endif
				sampledColorGradient = (SampledColorGradient2D) coroutineWithData.result;
			}
			else
				sampledColorGradient = SampledColorGradient2D.GenerateRandom(textureSize, colorAreaCount, blurRadius, borderSize);
		}

		void UpdateRenderer ()
		{
			// spriteRenderer.sprite = sampledColorGradient.MakeSprite(textureSize, pixelsPerUnit);
			MakeBlendedSprite ();
		}

		public void MakeBlendedSprite ()
		{
			if (sampledColorGradient == null)
				return;
			List<Vector2Int> pixelsToBlendFrom = new List<Vector2Int>();
			List<Color> colorsToBlendTo = new List<Color>();
			foreach (SampledColorGradient2DRenderer blendToColorGradientRenderer in blendColorGradientRenderers)
			{
				if (blendToColorGradientRenderer.spriteRenderer.sprite != null)
				{
					if (blendToColorGradientRenderer.trs.position.x > trs.position.x)
					{
						for (int y = 0; y < textureSize.y; y ++)
						{
							for (int x = 0; x <= blendRange; x ++)
							{
								Vector2Int pixelToBlendFrom = new Vector2Int(textureSize.x - x, y);
								int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
								Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(0, y);
								if (indexOfPixelToBlendFrom != -1)
								{
									colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], (x + 0) * (1f / blendRange));
									colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
									pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
								}
								else
									colorToBlendTo = Color.Lerp(colorToBlendTo, sampledColorGradient.GetColor(pixelToBlendFrom), (x + 0) * (1f / blendRange));
								pixelsToBlendFrom.Add(pixelToBlendFrom);
								colorsToBlendTo.Add(colorToBlendTo);
							}
						}
					}
					else if (blendToColorGradientRenderer.trs.position.x < trs.position.x)
					{
						for (int y = 0; y < textureSize.y; y ++)
						{
							for (int x = 0; x <= blendRange; x ++)
							{
								Vector2Int pixelToBlendFrom = new Vector2Int(x, y);
								int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
								Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(textureSize.x - 1, y);
								if (indexOfPixelToBlendFrom != -1)
								{
									colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], (x + 0) * (1f / blendRange));
									colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
									pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
								}
								else
									colorToBlendTo = Color.Lerp(colorToBlendTo, sampledColorGradient.GetColor(pixelToBlendFrom), (x + 0) * (1f / blendRange));
								pixelsToBlendFrom.Add(pixelToBlendFrom);
								colorsToBlendTo.Add(colorToBlendTo);
							}
						}
					}
					if (blendToColorGradientRenderer.trs.position.y > trs.position.y)
					{
						for (int x = 0; x < textureSize.x; x ++)
						{
							for (int y = 0; y <= blendRange; y ++)
							{
								Vector2Int pixelToBlendFrom = new Vector2Int(x, textureSize.y - y);
								int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
								Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(x, 0);
								if (indexOfPixelToBlendFrom != -1)
								{
									colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], (y + 0) * (1f / blendRange));
									colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
									pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
								}
								else
									colorToBlendTo = Color.Lerp(colorToBlendTo, sampledColorGradient.GetColor(pixelToBlendFrom), (y + 0) * (1f / blendRange));
								pixelsToBlendFrom.Add(pixelToBlendFrom);
								colorsToBlendTo.Add(colorToBlendTo);
							}
						}
					}
					else if (blendToColorGradientRenderer.trs.position.y < trs.position.y)
					{
						for (int x = 0; x < textureSize.x; x ++)
						{
							for (int y = 0; y <= blendRange; y ++)
							{
								Vector2Int pixelToBlendFrom = new Vector2Int(x, y);
								int indexOfPixelToBlendFrom = pixelsToBlendFrom.IndexOf(pixelToBlendFrom);
								Color colorToBlendTo = blendToColorGradientRenderer.spriteRenderer.sprite.texture.GetPixel(x, textureSize.y - 1);
								if (indexOfPixelToBlendFrom != -1)
								{
									colorToBlendTo = Color.Lerp(colorToBlendTo, colorsToBlendTo[indexOfPixelToBlendFrom], (y + 0) * (1f / blendRange));
									colorsToBlendTo.RemoveAt(indexOfPixelToBlendFrom);
									pixelsToBlendFrom.RemoveAt(indexOfPixelToBlendFrom);
								}
								else
									colorToBlendTo = Color.Lerp(colorToBlendTo, sampledColorGradient.GetColor(pixelToBlendFrom), (y + 0) * (1f / blendRange));
								pixelsToBlendFrom.Add(pixelToBlendFrom);
								colorsToBlendTo.Add(colorToBlendTo);
							}
						}
					}
				}
			}
			spriteRenderer.sprite = sampledColorGradient.MakeSprite(textureSize, pixelsPerUnit, colorsToBlendTo, pixelsToBlendFrom);
		}
	}
}