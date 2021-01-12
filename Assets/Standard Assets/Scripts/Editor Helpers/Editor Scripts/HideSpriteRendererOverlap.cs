#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEditor;

public class HideSpriteRendererOverlap : EditorScript
{
	[MenuItem("Tools/Hide Sprite Renderer Overlap %#o")]
	public static void Do ()
	{
		SpriteRenderer[] spriteRenderers = new SpriteRenderer[0];
		foreach (Transform trs in Selection.transforms)
		{
			SpriteRenderer spriteRenderer = trs.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				foreach (SpriteRenderer previousSpriteRenderer in spriteRenderers)
				{
					if (previousSpriteRenderer.bounds.Intersects(spriteRenderer.bounds))
					{
						
					}
				}
				spriteRenderers = spriteRenderers.Add(spriteRenderer);
			}
		}
	}
}

[CustomEditor(typeof(HideSpriteRendererOverlap))]
public class HideSpriteRendererOverlapEditor : EditorScriptEditor
{
}
#endif