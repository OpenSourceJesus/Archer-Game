#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//[ExecuteInEditMode]
public class MakeBlockable : MonoBehaviour
{
	void Start ()
	{
		Renderer renderer = GetComponent<Renderer>();
		SpriteRenderer spriteRenderer1 = renderer as SpriteRenderer;
		GameObject newGo = new GameObject();
		newGo.name = "Graphics Blocker";
		if (spriteRenderer1 != null)
		{
			spriteRenderer1.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
			SpriteRenderer spriteRenderer2 = newGo.AddComponent<SpriteRenderer>();
			spriteRenderer2.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
		}
		else
		{
			TilemapRenderer tilemapRenderer1 = renderer as TilemapRenderer;
			if (tilemapRenderer1 != null)
			{
				tilemapRenderer1.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
				TilemapRenderer tilemapRenderer2 = newGo.AddComponent<TilemapRenderer>();
				tilemapRenderer2.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			}
		}
		GraphicsBlocker blocker = newGo.AddComponent<GraphicsBlocker>();
		Collider2D collider = GetComponent<Collider2D>();
		BoxCollider2D boxCollider1 = collider as BoxCollider2D;
		if (boxCollider1 != null)
		{
			BoxCollider2D boxCollider2 = newGo.AddComponent<BoxCollider2D>();
			boxCollider2.size = boxCollider1.size;
			boxCollider2.offset = boxCollider1.offset;
			boxCollider2.edgeRadius = boxCollider1.edgeRadius;
			boxCollider2.isTrigger = true;
		}
		else
		{
			CircleCollider2D circleCollider1 = collider as CircleCollider2D;
			if (circleCollider1 != null)
			{
				CircleCollider2D circleCollider2 = newGo.AddComponent<CircleCollider2D>();
				circleCollider2.radius = circleCollider1.radius;
				circleCollider2.isTrigger = true;
			}
			else
			{
				PolygonCollider2D polygonCollider1 = collider as PolygonCollider2D;
				if (polygonCollider1 != null)
				{
					PolygonCollider2D polygonCollider2 = newGo.AddComponent<PolygonCollider2D>();
					polygonCollider2.points = polygonCollider1.points;
					polygonCollider2.isTrigger = true;
				}
				else
				{
					CapsuleCollider2D capsuleCollider1 = collider as CapsuleCollider2D;
					if (capsuleCollider1 != null)
					{
						CapsuleCollider2D capsuleCollider2 = newGo.AddComponent<CapsuleCollider2D>();
						capsuleCollider2.size = capsuleCollider1.size;
						capsuleCollider2.offset = capsuleCollider1.offset;
						capsuleCollider2.direction = capsuleCollider1.direction;
						capsuleCollider2.isTrigger = true;
					}
					else
					{
						TilemapCollider2D tilemapCollider1 = collider as TilemapCollider2D;
						if (tilemapCollider1 != null)
						{
							TilemapCollider2D tilemapCollider2 = newGo.AddComponent<TilemapCollider2D>();
							tilemapCollider2.offset = tilemapCollider1.offset;
							tilemapCollider2.isTrigger = true;
						}
					}
				}
			}
		}
		Transform newTrs = newGo.GetComponent<Transform>();
		newTrs.SetParent(transform);
		newTrs.localPosition = Vector3.zero;
		newTrs.localEulerAngles = Vector3.zero;
		newTrs.localScale = Vector3.one;
		blocker.Start ();
		DestroyImmediate(this);
	}
}
#endif