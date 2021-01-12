using System.Collections;
using UnityEngine;
using Extensions;
using UnityEngine.Tilemaps;

//[ExecuteInEditMode]
public class GraphicsBlocker : MonoBehaviour, IBlockable, ICollisionHandler
{
	public Renderer Renderer
	{
		get
		{
			return renderer;
		}
		set
		{
			renderer = value;
		}
	}
	public new Renderer renderer;
	public Renderer rootRenderer;
	public SpriteRenderer rootSpriteRenderer;
	public SpriteRenderer spriteRenderer;
	public TilemapRenderer rootTilemapRenderer;
	public TilemapRenderer tilemapRenderer;
	public Tilemap rootTilemap;
	public Tilemap tilemap;
	public Collider2D Collider
	{
		get
		{
			return collider;
		}
		set
		{
			collider = value;
		}
	}
	public new Collider2D collider;
	
#if UNITY_EDITOR
	void Reset ()
	{
		Start ();
	}
	
	public void Start ()
	{
		if (rootRenderer == null)
		{
			Transform parent = GetComponent<Transform>();
			//rootRenderer = parent.GetComponent<Renderer>();
			while (rootRenderer == null)
			{
				parent = parent.parent;
				if (parent != null)
					rootRenderer = parent.GetComponent<Renderer>();
			}
		}
		if (collider == null)
			collider = GetComponentInChildren<Collider2D>();
		if (renderer == null)
			renderer = GetComponentInChildren<Renderer>();
		if (rootSpriteRenderer == null)
			rootSpriteRenderer = rootRenderer as SpriteRenderer;
		if (rootTilemapRenderer == null)
			rootTilemapRenderer = rootRenderer as TilemapRenderer;
		if (spriteRenderer == null)
			spriteRenderer = renderer as SpriteRenderer;
		if (tilemapRenderer == null)
			tilemapRenderer = renderer as TilemapRenderer;
		if (tilemap == null)
			tilemap = GetComponent<Tilemap>();
		if (rootTilemap == null)
			rootTilemap = rootRenderer.GetComponent<Tilemap>();
	}
#endif
	
	void OnEnable ()
	{
		StopAllCoroutines ();
		SpriteRenderer spriteRenderer = renderer as SpriteRenderer;
		if (spriteRenderer != null)
			StartCoroutine(UpdateSpriteRenderer (spriteRenderer));
#if UNITY_EDITOR
		else if (!Application.isPlaying)
		{
			TilemapRenderer tilemapRenderer = renderer as TilemapRenderer;
			if (tilemapRenderer != null)
				StartCoroutine(UpdateTilemapRenderer (tilemapRenderer));
		}
#endif
	}
	
	IEnumerator UpdateSpriteRenderer (SpriteRenderer spriteRenderer)
	{
		while (true)
		{
			spriteRenderer.enabled = rootSpriteRenderer.enabled;
			spriteRenderer.sprite = rootSpriteRenderer.sprite;
			spriteRenderer.color = rootSpriteRenderer.color.DivideAlpha(2);
			spriteRenderer.flipX = rootSpriteRenderer.flipX;
			spriteRenderer.flipY = rootSpriteRenderer.flipY;
			spriteRenderer.sortingOrder = rootRenderer.sortingOrder;
			spriteRenderer.sortingLayerName = rootRenderer.sortingLayerName;
			yield return new WaitForEndOfFrame();
		}
	}
	
	IEnumerator UpdateTilemapRenderer (TilemapRenderer tilemapRenderer)
	{
		while (true)
		{
			tilemapRenderer.enabled = rootRenderer.enabled;
			tilemap.color = rootTilemap.color.DivideAlpha(2);
			TileBase[] tiles = rootTilemap.GetTilesBlock(rootTilemap.cellBounds);
			tilemap.SetTilesBlock(rootTilemap.cellBounds, tiles);
			tilemapRenderer.sortingOrder = rootRenderer.sortingOrder;
			tilemapRenderer.sortingLayerName = rootRenderer.sortingLayerName;
			yield return new WaitForEndOfFrame();
		}
	}
	
	public virtual void OnTriggerEnter2D (Collider2D other)
	{
		IBlockable blockable = other.GetComponent<IBlockable>();
		if (blockable != null)
		{
			foreach (GraphicsManager.GraphicsOverlap graphicsOverlap in GraphicsManager.Instance.graphicsOverlaps)
			{
				if ((graphicsOverlap.blockable1 == this && graphicsOverlap.blockable2 == blockable) || (graphicsOverlap.blockable1 == blockable && graphicsOverlap.blockable2 == this))
					return;
			}
			GraphicsManager.Instance.graphicsOverlaps.Add(new GraphicsManager.GraphicsOverlap(this, blockable));
		}
	}
	
	public virtual void OnTriggerExit2D (Collider2D other)
	{
		IBlockable blockable = other.GetComponent<IBlockable>();
		if (blockable != null)
		{
			GraphicsManager.GraphicsOverlap correspondingOverlap = null;
			foreach (GraphicsManager.GraphicsOverlap graphicsOverlap in GraphicsManager.Instance.graphicsOverlaps)
			{
				if ((graphicsOverlap.blockable1 == this && graphicsOverlap.blockable2 == blockable) || (graphicsOverlap.blockable1 == blockable && graphicsOverlap.blockable2 == this))
				{
					correspondingOverlap = graphicsOverlap;
					break;
				}
			}
			if (correspondingOverlap != null)
			{
				correspondingOverlap.End ();
				GraphicsManager.Instance.graphicsOverlaps.Remove(correspondingOverlap);
			}
		}
	}
	
	public virtual void OnTriggerEnter2DHandler (Collider2D other)
	{
		OnTriggerEnter2D (other);
	} 
	
	public virtual void OnTriggerStay2DHandler (Collider2D other)
	{
	}
	
	public virtual void OnTriggerExit2DHandler (Collider2D other)
	{
		OnTriggerExit2D (other);
	}
	
	public virtual void OnCollisionEnter2DHandler (Collision2D coll)
	{
	}
	
	public virtual void OnCollisionStay2DHandler (Collision2D coll)
	{
	}
	
	public virtual void OnCollisionExit2DHandler (Collision2D coll)
	{
	}
}