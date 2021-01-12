using UnityEngine;
using System;

[Serializable]
public class ImageEffect
{
	public Texture2D image;
	[Range(0, 1)]
	public float applyAmount;

	public virtual void Apply ()
	{
	}

	public virtual void Apply (Vector2Int pixelPosition, ref Color[] pixelColors)
	{
	}
}