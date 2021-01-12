#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Extensions;
using UnityEditor;
using ArcherGame;
using System.IO;

[RequireComponent(typeof(SpriteRenderer))]
public class CombineSpriteRenderers : EditorScript
{
	public SpriteRenderer spriteRenderer;
	public SpriteRenderer[] spriteRenderers = new SpriteRenderer[0];
	public string spriteAssetPath;
	public string textureAssetPath;
	public bool shouldMakeAssets;
	public bool makeAssetPathsUnique;
	public bool update;
	public int checksPerUnit;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (spriteRenderer != null)
				spriteRenderer = GetComponent<SpriteRenderer>();
			return;
		}
	}

	void OnValidate ()
	{
		DoEditorUpdate ();
	}

	public override void DoEditorUpdate ()
	{
		if (!update)
			return;
		update = false;
		Do ();
	}

	public virtual void Do ()
	{
		Rect[] imageWorldRects = new Rect[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i ++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
			imageWorldRects[i] = spriteRenderer.GetWorldRect(spriteRenderer.GetComponent<Transform>());
		}
		Rect textureWorldRect = RectExtensions.Combine(imageWorldRects);
		Vector2Int textureSize = (textureWorldRect.size * checksPerUnit).ToVec2Int();
		Color[] colors = new Color[textureSize.x * textureSize.y];
		Texture2D texture = new Texture2D(textureSize.x, textureSize.y);
		for (int x = 0; x < textureSize.x; x ++)
		{
			for (int y = 0; y < textureSize.y; y ++)
			{
				colors[x + y * textureSize.x] = ColorExtensions.CLEAR;
                for (int i = 0; i < spriteRenderers.Length; i ++)
				{
                    SpriteRenderer spriteRenderer = spriteRenderers[i];
                    Vector2 checkPosition = Rect.NormalizedToPoint(textureWorldRect, new Vector2(x, y).Divide(textureSize));
					Color color = spriteRenderer.ColorAtWorldPosition(checkPosition, spriteRenderer.GetComponent<Transform>());
					if (color != ColorExtensions.NULL && color.a > 0)
					{
						colors[x + y * textureSize.x] = color;
						break;
					}
				}
			}
		}
		texture.SetPixels(colors);
		texture.Apply();
		if (shouldMakeAssets)
		{
			if (makeAssetPathsUnique)
			{
				string newAssetPath = textureAssetPath;
				while (File.Exists(newAssetPath))
					newAssetPath = newAssetPath.Replace(".asset", "1.asset");
				textureAssetPath = newAssetPath;
				AssetDatabase.CreateAsset(texture, newAssetPath);
			}
			else
				AssetDatabase.CreateAsset(texture, textureAssetPath);
			AssetDatabase.Refresh();
		}
		Sprite sprite = Sprite.Create(texture, Rect.MinMaxRect(0, 0, texture.width, texture.height), Vector2.one / 2, checksPerUnit);
		if (shouldMakeAssets)
		{
			if (makeAssetPathsUnique)
			{
				string newAssetPath = spriteAssetPath;
				while (File.Exists(newAssetPath))
					newAssetPath = newAssetPath.Replace(".asset", "1.asset");
				spriteAssetPath = newAssetPath;
				AssetDatabase.CreateAsset(sprite, newAssetPath);
			}
			else
				AssetDatabase.CreateAsset(sprite, spriteAssetPath);
			AssetDatabase.Refresh();
		}
		if (spriteRenderer != null)
			spriteRenderer.sprite = sprite;
	}
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CombineSpriteRenderers))]
public class CombineSpriteRenderersEditor : EditorScriptEditor
{
}
#else
public class CombineSpriteRenderers : EditorScript
{
}
#endif