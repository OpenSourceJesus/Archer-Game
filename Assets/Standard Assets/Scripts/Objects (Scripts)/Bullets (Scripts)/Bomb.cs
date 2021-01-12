using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	public class Bomb : Bullet
	{
		public Transform explodeTrs;
		public float explodeSize;
		public float explodeDuration;
        public Animator anim;
		public bool explodeOnHit;
		[HideInInspector]
		public Vector2 startPoint;
		[HideInInspector]
		public Vector2 destination;

        public override void OnTriggerEnter2D (Collider2D other)
        {
			if (explodeOnHit)
				Explode ();
		}

		public virtual void Explode ()
		{
			StartCoroutine(ExplodeRoutine ());
		}

		public virtual IEnumerator ExplodeRoutine ()
		{
			spriteRenderer.enabled = false;
			explodeTrs.SetParent(null);
			explodeTrs.gameObject.SetActive(true);
			do
			{
				explodeTrs.localScale += Vector3.one * explodeSize * Time.deltaTime / explodeDuration;
				yield return new WaitForEndOfFrame();
			} while (explodeTrs.localScale.x < explodeSize);
			explodeTrs.gameObject.SetActive(false);
			explodeTrs.SetParent(trs);
			explodeTrs.localScale = Vector3.zero;
			spriteRenderer.enabled = true;
			ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
		}
	}
}