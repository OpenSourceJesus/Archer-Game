using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using ArcherGame;

public class GraphicsManager : SingletonMonoBehaviour<GraphicsManager>
{
	public Material[] filters;
	public Transform spriteMaskTrsPrefab;
	public List<GraphicsOverlap> graphicsOverlaps = new List<GraphicsOverlap>();
	public Material gameViewMaterial;
	Texture2D image;
	RenderTexture renderTexture;
	new Camera camera;
	
	public override void Awake ()
	{
		if (filters == null || filters.Length == 0)
			return;
		GraphicsManager graphicsManager = GameCamera.Instance.camera.gameObject.AddComponent<GraphicsManager>();
		graphicsManager.filters = filters;
		graphicsManager.spriteMaskTrsPrefab = spriteMaskTrsPrefab;
		if (GameManager.singletons.ContainsKey(GetType()))
			GameManager.singletons[GetType()] = this;
		else
			GameManager.singletons.Add(GetType(), this);
		enabled = false;
	}

	IEnumerator Start ()
	{
		do
		{
			camera = GameCamera.Instance.camera;
			yield return new WaitForEndOfFrame();
		} while (camera == null);
		renderTexture = camera.targetTexture;
		if (renderTexture == null)
			yield break;
		image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		// RenderTexture.active = renderTexture;
		gameViewMaterial.mainTexture = image;
	}
	
	public virtual void DoUpdate ()
	{
		for (int i = 0; i < graphicsOverlaps.Count; i ++)
		{
			GraphicsOverlap graphicsOverlap = graphicsOverlaps[i];
			graphicsOverlap.Update ();
		}
		// Texture2D image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		// Camera camera = GameCamera.Instance.camera;
		// RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
		// camera.targetTexture = renderTexture;
		camera.Render();
		RenderTexture.active = renderTexture;
		image.ReadPixels(Rect.MinMaxRect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		image.Apply();
		// camera.targetTexture = null;
		RenderTexture.active = null;
		// DestroyImmediate(renderTexture);
		// gameViewMaterial.mainTexture = image;
	}
	
	public virtual void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		for (int i = 0; i < filters.Length; i ++)
		{
			Material filter = filters[i];
			Graphics.Blit(source, destination, filter);
		}
	}
	
	public class GraphicsOverlap
	{
		public IBlockable blockable1;
		public IBlockable blockable2;
		public Transform maskTrs;
		
		public GraphicsOverlap (IBlockable blockable1, IBlockable blockable2)
		{
			this.blockable1 = blockable1;
			this.blockable2 = blockable2;
			maskTrs = Instantiate(GraphicsManager.Instance.spriteMaskTrsPrefab);
		}
		
		public virtual Rect GetOverlap ()
		{
			if (blockable1.Collider != null && blockable1.Collider.isActiveAndEnabled && blockable2.Collider != null && blockable2.Collider.isActiveAndEnabled)
			{
				Rect rect1 = blockable1.Collider.bounds.ToRect();
				Rect rect2 = blockable2.Collider.bounds.ToRect();
				Vector2 overlapRectMin = new Vector2(Mathf.Max(rect1.min.x, rect2.min.x), Mathf.Max(rect1.min.y, rect2.min.y));
				Vector2 overlapRectMax = new Vector2(Mathf.Min(rect1.max.x, rect2.max.x), Mathf.Min(rect1.max.y, rect2.max.y));
				return Rect.MinMaxRect(overlapRectMin.x, overlapRectMin.y, overlapRectMax.x, overlapRectMax.y);
			}
			else
			{
				End ();
				return RectExtensions.NULL;
			}
		}
		
		public virtual void Update ()
		{
			Rect overlapRect = GetOverlap();
			if (maskTrs != null)
			{
				maskTrs.position = overlapRect.center;
				maskTrs.localScale = overlapRect.size.SetZ(1);
			}
		}
		
		public virtual void End ()
		{
			if (maskTrs != null)
				Destroy (maskTrs.gameObject);
		}
	}
}