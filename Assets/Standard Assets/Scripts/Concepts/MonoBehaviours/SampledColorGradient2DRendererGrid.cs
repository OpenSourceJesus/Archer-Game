using UnityEngine;
using System.Collections.Generic;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class SampledColorGradient2DRendererGrid : MonoBehaviour
	{
		public RectInt rect;
		[HideInInspector]
		public RectInt previousRect;
		public Transform trs;
		public Dictionary<Vector2Int, SampledColorGradient2DRenderer> renderersDict = new Dictionary<Vector2Int, SampledColorGradient2DRenderer>();
		public SampledColorGradient2DRenderer defaultRenderer;

#if UNITY_EDITOR
		public bool setSize;
		public bool resetRenderersDict;
		void OnValidate ()
		{
			if (Application.isPlaying)
				return;
			if (setSize)
			{
				setSize = false;
				SetSize ();
			}
			if (resetRenderersDict)
			{
				resetRenderersDict = false;
				ResetRenderersDict ();
			}
		}
#endif

		void SetSize ()
		{
			foreach (Vector2Int position in previousRect.allPositionsWithin)
			{
				if (!rect.Contains(position))
					RemoveRenderer (position);
			}
			foreach (Vector2Int position in rect.allPositionsWithin)
			{
				if (!previousRect.Contains(position))
					MakeNewRenderer (position);
			}
			previousRect = rect.DeepCopyByExpressionTree<RectInt>();
		}

		void MakeNewRenderer (Vector2Int position)
		{
			Vector2 worldPosition = position.Multiply((Vector2) defaultRenderer.textureSize / defaultRenderer.pixelsPerUnit);
			SampledColorGradient2DRenderer renderer = new GameObject().AddComponent<SampledColorGradient2DRenderer>();
			renderer.textureSize = defaultRenderer.textureSize;
			renderer.pixelsPerUnit = defaultRenderer.pixelsPerUnit;
			renderer.blurRadius = defaultRenderer.blurRadius;
			renderer.colorAreaCount = defaultRenderer.colorAreaCount;
			renderer.blendRange = defaultRenderer.blendRange;
			renderer.waitUntilDone = defaultRenderer.waitUntilDone;
			renderer.borderSize = defaultRenderer.borderSize;
			renderer.spriteRenderer = renderer.GetComponent<SpriteRenderer>();
			renderer.spriteRenderer.sortingLayerID = defaultRenderer.spriteRenderer.sortingLayerID;
			renderer.spriteRenderer.sortingOrder = defaultRenderer.spriteRenderer.sortingOrder;
			renderer.spriteRenderer.color = defaultRenderer.spriteRenderer.color;
			renderer.trs = renderer.GetComponent<Transform>();
			renderer.trs.localPosition = worldPosition;
			renderer.trs.SetParent(trs);
			renderer.GenerateRandom ();
			SampledColorGradient2DRenderer blendRenderer;
			if (renderersDict.TryGetValue(position + Vector2Int.right, out blendRenderer))
			{
				renderer.blendColorGradientRenderers = renderer.blendColorGradientRenderers.Add(blendRenderer);
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Add(renderer);
			}
			if (renderersDict.TryGetValue(position + Vector2Int.left, out blendRenderer))
			{
				renderer.blendColorGradientRenderers = renderer.blendColorGradientRenderers.Add(blendRenderer);
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Add(renderer);
			}
			if (renderersDict.TryGetValue(position + Vector2Int.up, out blendRenderer))
			{
				renderer.blendColorGradientRenderers = renderer.blendColorGradientRenderers.Add(blendRenderer);
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Add(renderer);
			}
			if (renderersDict.TryGetValue(position + Vector2Int.down, out blendRenderer))
			{
				renderer.blendColorGradientRenderers = renderer.blendColorGradientRenderers.Add(blendRenderer);
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Add(renderer);
			}
			// renderer.MakeBlendedSprite ();
			renderersDict.Add(position, renderer);
		}

		void RemoveRenderer (Vector2Int position)
		{
			SampledColorGradient2DRenderer renderer;
			if (!renderersDict.TryGetValue(position, out renderer))
				return;
			SampledColorGradient2DRenderer blendRenderer;
			if (renderersDict.TryGetValue(position + Vector2Int.right, out blendRenderer))
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Remove(renderer);
			if (renderersDict.TryGetValue(position + Vector2Int.left, out blendRenderer))
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Remove(renderer);
			if (renderersDict.TryGetValue(position + Vector2Int.up, out blendRenderer))
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Remove(renderer);
			if (renderersDict.TryGetValue(position + Vector2Int.down, out blendRenderer))
				blendRenderer.blendColorGradientRenderers = blendRenderer.blendColorGradientRenderers.Remove(renderer);
			DestroyImmediate(renderer.gameObject);
			renderersDict.Remove(position);
		}

		void ResetRenderersDict ()
		{
			renderersDict.Clear();
			foreach (Transform child in trs)
				renderersDict.Add(child.localPosition.Divide((Vector2) defaultRenderer.textureSize / defaultRenderer.pixelsPerUnit).ToVec2Int(), child.GetComponent<SampledColorGradient2DRenderer>());
		}
	}
}