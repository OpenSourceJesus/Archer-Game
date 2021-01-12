using System.Collections.Generic;
using UnityEngine;
using Extensions;
using DelayedDespawn = ArcherGame.ObjectPool.DelayedDespawn;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Arrow : MonoBehaviour, IUpdatable, ISpawnable
	{
		public Transform trs;
		public new Collider2D collider;
		public Rigidbody2D rigid;
		public virtual bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public float damageMultiplier;
		public float minSpeedToRotateSqr;
		[HideInInspector]
		public DelayedDespawn delayedDespawn;
		[HideInInspector]
		public float speedSqr;
		public static List<Arrow> shotArrows = new List<Arrow>();
		public Player owner;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (collider == null)
					collider = GetComponent<Collider2D>();
				if (rigid == null)
					rigid = GetComponent<Rigidbody2D>();
				return;
			}
#endif
			collider.isTrigger = false;
			rigid.bodyType = RigidbodyType2D.Dynamic;
			GameManager.updatables = GameManager.updatables.Add(this);
			shotArrows.Add(this);
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			if (coll.gameObject.GetComponent<Enemy>() != null)
			{
				GameManager.updatables = GameManager.updatables.Remove(this);
				rigid.bodyType = RigidbodyType2D.Kinematic;
				rigid.velocity = Vector2.zero;
				collider.isTrigger = true;
				trs.SetParent(coll.transform);
			}
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DelayDespawnMe ()
		{
			ObjectPool.instance.CancelDelayedDespawn (delayedDespawn);
			delayedDespawn = ObjectPool.instance.DelayDespawn (prefabIndex, gameObject, trs, delayedDespawn.duration);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
			int indexOfArrowEntry = owner.GetArrowEntryIndex(GetType());
			owner.arrowEntries[indexOfArrowEntry].holdingCount ++;
			owner.arrowEntries[indexOfArrowEntry].arrows = owner.arrowEntries[indexOfArrowEntry].arrows.Remove(this);
			shotArrows.Remove(this);
		}

		public virtual void DoUpdate ()
		{
			speedSqr = rigid.velocity.sqrMagnitude;
			if (speedSqr >= minSpeedToRotateSqr)
				trs.up = rigid.velocity;
		}
	}
}