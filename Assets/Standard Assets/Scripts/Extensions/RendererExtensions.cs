using UnityEngine;

namespace Extensions
{
	public static class RendererExtensions
	{
		// TODO: Take into account sprite flipping
		public static Color ColorAtWorldPosition (this SpriteRenderer spriteRenderer, Vector2 worldPosition, Transform trs)
		{
			Vector2Int pixel = spriteRenderer.WorldToPixelPosition(worldPosition, trs);
			if (spriteRenderer.sprite.textureRect.Contains(pixel))
				return spriteRenderer.sprite.texture.GetPixel(pixel.x, pixel.y);
			else
				return ColorExtensions.NULL;
		}
		
		// TODO: Take into account sprite flipping
		public static Vector2Int WorldToPixelPosition (this SpriteRenderer spriteRenderer, Vector2 worldPosition, Transform trs)
		{
			Rect worldRect = spriteRenderer.GetWorldRect(trs);
			Texture2D texture = spriteRenderer.sprite.texture;
			Vector2Int textureSize = new Vector2Int(texture.width, texture.height);
			Vector2 normalizedPosition = Rect.PointToNormalized(worldRect, worldPosition);
			normalizedPosition = normalizedPosition.Rotate(spriteRenderer.sprite.pivot.Divide(textureSize.ToVec2()), trs.eulerAngles.z);
			return textureSize.Multiply(normalizedPosition);
		}
		
		// TODO: Take into account sprite flipping
		public static Vector2 PixelToWorldPosition (this SpriteRenderer spriteRenderer, Vector2Int pixelPosition, Transform trs)
		{
			Rect pixelRect = spriteRenderer.sprite.rect;
			Vector2 normalizedPosition = Rect.PointToNormalized(pixelRect, pixelPosition);
			Rect worldRect = spriteRenderer.GetWorldRect(trs);
			return Rect.NormalizedToPoint(worldRect, normalizedPosition);
		}

		// TODO: Take into account rotation in edit mode
		public static Rect GetWorldRect (this SpriteRenderer spriteRenderer, Transform trs)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Vector2Int textureSize = spriteRenderer.sprite.textureRect.size.ToVec2Int();
				Rect output = Rect.MinMaxRect(-textureSize.x / 2, -textureSize.y / 2, textureSize.x / 2, textureSize.y / 2);
				output.size = output.size / spriteRenderer.sprite.pixelsPerUnit;
				output.size = output.size.Multiply(trs.lossyScale);
				output = output.AnchorToPoint(trs.position, spriteRenderer.sprite.pivot.Divide(textureSize.ToVec2()));
				return output;
			}
#endif
			return spriteRenderer.bounds.ToRect();
		}
	}
}