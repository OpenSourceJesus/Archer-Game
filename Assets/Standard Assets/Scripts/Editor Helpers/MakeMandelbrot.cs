#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using UnityEditor;
using Extensions;
using System.Collections.Generic;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class MakeMandelbrot : MonoBehaviour
{
	public Vector2 from;
	public Vector2 to;
	public float zoom;
	public int maxIterations;
	public Gradient gradient;
	public Vector2Int textureSize;
	public Transform trs;
	public SpriteRenderer spriteRenderer;
	public int pixelsPerUnit;
	public float numerator;
	public bool makeAssets;
	public string textureAssetPath;
	public string spriteAssetPath;
	public bool makeAssetPathsUnique;
	public EditorCoroutine editorCoroutine;
	public bool waitUntilDone;

	void OnDisable ()
	{
		if (editorCoroutine != null)
			EditorCoroutineUtility.StopCoroutine(editorCoroutine);
	}

	void OnValidate ()
	{
		if (!enabled)
			return;
		textureSize = textureSize.SetToMinComponents(Vector2Int.one * SystemInfo.maxTextureSize);
		zoom = 1f / (to.magnitude - from.magnitude);
		if (editorCoroutine != null)
			EditorCoroutineUtility.StopCoroutine(editorCoroutine);
		editorCoroutine = EditorCoroutineUtility.StartCoroutine(Do (), this);
	}

	IEnumerator Do ()
	{
		Texture2D texture = new Texture2D(textureSize.x, textureSize.y);
		Color[] colors = new Color[textureSize.x * textureSize.y];
		int index = 0;
		for (int x = 0; x < textureSize.x; x ++)
		{
			for (int y = 0; y < textureSize.y; y ++)
			{
				float x2 = (to.x + from.x) / 2 + ((x + trs.position.x * textureSize.x) - textureSize.x / 2) * (1f / zoom);
				float y2 = (to.y + from.y) / 2 + ((y + trs.position.y * textureSize.y) - textureSize.y / 2) * (1f / zoom);
				colors[y * textureSize.x + x] = gradient.Evaluate(numerator / GetIterationCount(x2, y2));
				index ++;
				if (!waitUntilDone)
				{
					// print((float) index / colors.Length * 100);
					print(index + "/" + colors.Length);
					yield return new WaitForEndOfFrame();
				}
			}
		}
		texture.SetPixels(colors);
		texture.Apply();
		Sprite sprite = Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize.x, textureSize.y), Vector2.one / 2, pixelsPerUnit);
		if (spriteRenderer != null)
			spriteRenderer.sprite = sprite;
		if (makeAssets)
		{
			if (makeAssetPathsUnique)
			{
				string newAssetPath = textureAssetPath;
				while (File.Exists(newAssetPath))
					newAssetPath = newAssetPath.Replace(".asset", "1.asset");
				textureAssetPath = newAssetPath;
				AssetDatabase.CreateAsset(texture, newAssetPath);
				newAssetPath = spriteAssetPath;
				while (File.Exists(newAssetPath))
					newAssetPath = newAssetPath.Replace(".asset", "1.asset");
				spriteAssetPath = newAssetPath;
				AssetDatabase.CreateAsset(sprite, newAssetPath);
			}
			else
			{
				AssetDatabase.CreateAsset(texture, textureAssetPath);
				AssetDatabase.CreateAsset(sprite, spriteAssetPath);
			}
			AssetDatabase.Refresh();
		}
		yield break;
	}

	int GetIterationCount (float cx, float cy)
	{
		int result = 0;
		float x = 0.0f;
		float y = 0.0f;
		float xx = 0.0f, yy = 0.0f;
		while (xx + yy <= 4.0f && result < maxIterations) // are we out of control disk?
		{
			xx = x * x;
			yy = y * y;
			float xtmp = xx - yy + cx;
			y = 2.0f * x * y + cy; // computes z^2 + c
			x = xtmp;
			result ++;
		}
		return result;
	}
}
#else
using UnityEngine;

public class MakeMandelbrot : MonoBehaviour
{
}
#endif