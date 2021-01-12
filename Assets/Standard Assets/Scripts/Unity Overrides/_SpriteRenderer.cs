using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	[RequireComponent(typeof(SpriteRenderer))]
	[DisallowMultipleComponent]
	public class _SpriteRenderer : MonoBehaviour
	{
		public SpriteRenderer spriteRenderer;
		public Vector2 initFacing;
		Vector2 facing;
		
		void Awake ()
		{
			if (!Application.isPlaying)
			{
				spriteRenderer = GetComponent<SpriteRenderer>();
				return;
			}
			facing = initFacing;
		}
		
		public void FlipTowards (Transform trs)
		{
			Vector2 toTrs = trs.position - transform.position;
			if (MathfExtensions.AreOppositeSigns(facing.x, toTrs.x))
			{
				spriteRenderer.flipX = !spriteRenderer.flipX;
				facing.x *= -1;
			}
			if (MathfExtensions.AreOppositeSigns(facing.y, toTrs.y))
			{
				spriteRenderer.flipY = !spriteRenderer.flipY;
				facing.y *= -1;
			}
		}
	}
}