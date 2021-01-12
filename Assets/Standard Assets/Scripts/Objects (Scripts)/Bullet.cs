using UnityEngine;
using Extensions;
using DelayedDespawn = ArcherGame.ObjectPool.DelayedDespawn;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Bullet : MonoBehaviour, ISpawnable
	{
		public Transform trs;
		public Rigidbody2D rigid;
		public Collider2D collider;
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
        public float moveSpeed;
        public float lifetime;
        DelayedDespawn delayedDespawn;
		public SpriteRenderer spriteRenderer;
		public Color canRetargetColor;
		public Color cantRetargetColor;
		public bool despawnOnHit;
		[HideInInspector]
		public int timesRetargeted;
		[HideInInspector]
		public bool hasFinishedRetargeting;
		[HideInInspector]
		public Vector2 boundsSize = VectorExtensions.NULL3;

        public virtual void OnEnable ()
        {
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (rigid == null)
					rigid = GetComponent<Rigidbody2D>();
				if (collider == null)
					collider = GetComponent<Collider2D>();
				if (boundsSize == (Vector2) VectorExtensions.NULL3)
					boundsSize = collider.GetSize(trs);
				return;
			}
#endif
            rigid.velocity = trs.up * moveSpeed;
			if (lifetime > 0)
            	delayedDespawn = ObjectPool.instance.DelayDespawn (prefabIndex, gameObject, trs, lifetime);
        }

        public virtual void OnTriggerEnter2D (Collider2D other)
        {
            if (other == Player.instance.collider)
                Player.instance.TakeDamage ();
			if (despawnOnHit)
				Despawn ();
        }

		public virtual void Despawn ()
		{
			ObjectPool.instance.CancelDelayedDespawn (delayedDespawn);
			ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
		}

		public virtual void Retarget (Vector2 direction, bool lastRetarget = false)
		{
			if (lastRetarget && hasFinishedRetargeting)
				return;
			trs.up = direction;
			rigid.velocity = trs.up * moveSpeed;
			timesRetargeted ++;
			if (lastRetarget)
			{
				hasFinishedRetargeting = true;
				spriteRenderer.color = cantRetargetColor;
				spriteRenderer.sortingOrder --;
			}
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			timesRetargeted = 0;
			if (hasFinishedRetargeting)
			{
				hasFinishedRetargeting = false;
				spriteRenderer.color = canRetargetColor;
				spriteRenderer.sortingOrder ++;
			}
			StopAllCoroutines();
		}
	}
}