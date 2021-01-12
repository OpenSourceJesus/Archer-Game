using UnityEngine;
using System;
using Extensions;
using System.Collections;
using System.Collections.Generic;
using Ferr;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class DissolveTile : MonoBehaviour
	{
		public Transform trs;
		public SpriteRenderer spriteRenderer;
		public Ferr2DT_PathTerrain[] terrains = new Ferr2DT_PathTerrain[0];
		public new Collider2D collider;
		public Collider2D rebuildCollider;
		public float dissolveDuration;
		public float rebuildDelay;
		public float rebuildDuration;
		public LayerMask whatICantRebuildOn;
		int numberOfCollidersTouchingMe;

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			numberOfCollidersTouchingMe ++;
			if (numberOfCollidersTouchingMe == 1)
			{
				StopAllCoroutines();
				if (spriteRenderer != null)
					StartCoroutine(DissolveRoutine_SpriteRenderer ());
				else
					StartCoroutine(DissolveRoutine_Terrain ());
			}
		}

		public virtual void OnCollisionExit2D (Collision2D coll)
		{
			numberOfCollidersTouchingMe --;
			if (numberOfCollidersTouchingMe == 0)
			{
				StopAllCoroutines();
				if (spriteRenderer != null)
					StartCoroutine(RebuildRoutine_SpriteRenderer ());
				else
					StartCoroutine(RebuildRoutine_Terrain ());
			}
		}
		
		public virtual IEnumerator DissolveRoutine_SpriteRenderer ()
		{
			do
			{
				yield return new WaitForEndOfFrame();
				spriteRenderer.color = spriteRenderer.color.AddAlpha(-Time.deltaTime / dissolveDuration);
				if (spriteRenderer.color.a <= 0)
				{
					spriteRenderer.color = spriteRenderer.color.SetAlpha(0);
					break;
				}
			} while (true);
			collider.enabled = false;
		}
		
		public virtual IEnumerator DissolveRoutine_Terrain ()
		{
			do
			{
				yield return new WaitForEndOfFrame();
				for (int i = 0; i < terrains.Length; i ++)
				{
					Ferr2DT_PathTerrain terrain = terrains[i];
					terrain.vertexColor = terrain.vertexColor.AddAlpha(-Time.deltaTime / dissolveDuration);
					if (spriteRenderer.color.a <= 0)
					{
						terrain.vertexColor = terrain.vertexColor.SetAlpha(0);
						break;
					}
				}
			} while (true);
			collider.enabled = false;
		}

		public virtual IEnumerator RebuildRoutine_SpriteRenderer ()
		{
			yield return new WaitForSeconds(rebuildDelay);
			ContactFilter2D contactFilter = new ContactFilter2D();
			contactFilter.layerMask = whatICantRebuildOn;
			contactFilter.useLayerMask = true;
			yield return new WaitUntil(() => (Physics2D.OverlapCollider(rebuildCollider, contactFilter, new Collider2D[1]) == 0));
			collider.enabled = true;
			while (true)
			{
				yield return new WaitForEndOfFrame();
				spriteRenderer.color = spriteRenderer.color.AddAlpha(Time.deltaTime / rebuildDuration);
				if (spriteRenderer.color.a >= 1)
				{
					spriteRenderer.color = spriteRenderer.color.SetAlpha(1);
					break;
				}
			}
		}

		public virtual IEnumerator RebuildRoutine_Terrain ()
		{
			yield return new WaitForSeconds(rebuildDelay);
			ContactFilter2D contactFilter = new ContactFilter2D();
			contactFilter.layerMask = whatICantRebuildOn;
			contactFilter.useLayerMask = true;
			yield return new WaitUntil(() => (Physics2D.OverlapCollider(rebuildCollider, contactFilter, new Collider2D[1]) == 0));
			collider.enabled = true;
			do
			{
				yield return new WaitForEndOfFrame();
				for (int i = 0; i < terrains.Length; i ++)
				{
					Ferr2DT_PathTerrain terrain = terrains[i];
					terrain.vertexColor = terrain.vertexColor.AddAlpha(Time.deltaTime / rebuildDuration);
					if (terrain.vertexColor.a >= 1)
					{
						terrain.vertexColor = terrain.vertexColor.SetAlpha(1);
						break;
					}
				}
			} while (true);
		}
	}
}