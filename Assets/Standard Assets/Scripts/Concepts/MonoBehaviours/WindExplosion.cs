using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class WindExplosion : MonoBehaviour, IUpdatable, ISpawnable
	{
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		[HideInInspector]
		public float force;
		public Transform trs;
		public new Collider2D collider;
		public float duration;
		public float minForce;
		public float maxForce;
		public float forceMultiplier;
		public float forceToSizeMultiplier;
		[HideInInspector]
		public List<Collider2D> hitColliders = new List<Collider2D>();

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void DoUpdate ()
		{
			trs.localScale += Vector3.one * force * forceToSizeMultiplier * Time.deltaTime / duration;
			if (trs.localScale.x > force * forceToSizeMultiplier)
				ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			collider.enabled = false;
			trs.localScale = Vector3.zero;
			GameManager.updatables = GameManager.updatables.Remove(this);
			hitColliders.Clear();
		}

		public virtual void SetForce (float force)
		{
			this.force = Mathf.Clamp(force * forceMultiplier, minForce, maxForce);
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			if (hitColliders.Contains(other))
				return;
			hitColliders.Add(other);
			Vector2 addToVelocity = (Vector2) (other.GetComponent<Transform>().position - trs.position).normalized * force;
			PlatformerEntity platformerEntity = other.GetComponentInParent<PlatformerEntity>();
			if (platformerEntity != null)
			{
				platformerEntity.velocityEffectors_Vector2Dict["Wind Explosion"].effect += addToVelocity;
				AwakableEnemy awakableEnemy = platformerEntity as AwakableEnemy;
				if (awakableEnemy != null && !awakableEnemy.awake)
					awakableEnemy.Awaken ();
			}
			else
			{
				Rigidbody2D rigid = other.GetComponent<Rigidbody2D>();
				if (rigid != null)
				{
					if (GameManager.gameModifierDict["Unslowable Pull Arrows"].isActive)
					{
						Arrow arrow = other.GetComponent<Arrow>();
						if (arrow != null)
						{
							if ((arrow as PullArrow) == null)
								rigid.velocity += addToVelocity;
						}
						else
							rigid.velocity += addToVelocity;
					}
					else
						rigid.velocity += addToVelocity;
				}
				else
					Destroy(other.gameObject);
			}
		}
	}
}