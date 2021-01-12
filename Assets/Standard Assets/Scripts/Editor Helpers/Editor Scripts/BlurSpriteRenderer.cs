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
public class BlurSpriteRenderer : EditorScript
{
	public SpriteRenderer spriteRenderer;
	public int blurRadius;
	public bool update;
	Texture2D texture;
	Vector2Int textureSize;

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
		texture = spriteRenderer.sprite.texture;
		textureSize = new Vector2Int(texture.width, texture.height);
		texture.SetPixels(GetColors ());
		texture.Apply();
	}

	public virtual Color[] GetColors ()
	{
		Texture2D texture = spriteRenderer.sprite.texture;
		Vector2Int textureSize = new Vector2Int(texture.width, texture.height);
		Color[] output = new Color[textureSize.x * textureSize.y];
		for (int x = 0; x < textureSize.x; x ++)
		{
			for (int y = 0; y < textureSize.y; y ++)
				output[x + y * textureSize.x] = GetColor(new Vector2Int(x, y));
		}
		return output;
	}

	public virtual Color GetColor (Vector2Int position)
	{
		List<Color> colors = new List<Color>();
		for (int x = Mathf.Clamp(position.x - blurRadius, 0, textureSize.x); x <= Mathf.Clamp(position.x + blurRadius, 0, textureSize.x - 1); x ++)
		{
			for (int y = Mathf.Clamp(position.y - blurRadius, 0, textureSize.y); y <= Mathf.Clamp(position.y + blurRadius, 0, textureSize.y - 1); y ++)
			{
				if (new Vector2Int(x, y) != position)
					colors.Add(texture.GetPixel(x, y));
			}
		}
		return ColorExtensions.GetAverage(colors.ToArray());
	}
}

[CanEditMultipleObjects]
[CustomEditor(typeof(BlurSpriteRenderer))]
public class BlurSpriteRendererEditor : EditorScriptEditor
{
}
#else
public class BlurSpriteRenderer : EditorScript
{
}
#endif