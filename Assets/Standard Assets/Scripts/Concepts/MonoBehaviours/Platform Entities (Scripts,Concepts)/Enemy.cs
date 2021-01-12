using UnityEngine;
using Extensions;
using System.Collections;
using UnityEngine.U2D.Animation;

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class Enemy : PlatformerEntity, IDestructable, ISpawnable
	{
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public int prefabIndex;
		protected float hp;
		public float Hp
		{
			get
			{
				return hp;
			}
			set
			{
				hp = value;
			}
		}
		public uint maxHp;
		public uint MaxHp
		{
			get
			{
				return maxHp;
			}
			set
			{
				maxHp = value;
			}
		}
		public Transform healthbarTrs;
		public Shooter shooter;
		public EnemyBattle battleIAmPartOf;
		public Transform flipToPlayerX_Trs;
		public delegate void OnDeath();
		public event OnDeath onDeath;
		public Blood bloodPrefab;
		float previousToPlayerX;
		public float decayDelay;
		public SpriteSkin spriteSkin;
		public GameObject activateOnDeath;
		public Collider2D deadCollider;
		[HideInInspector]
		public bool isDead;
		public string attackAnimTriggerName = "toATTACK";
		public string deathAnimTriggerName = "toDYING";
		[HideInInspector]
		public Vector2 initPosition;
		[HideInInspector]
		public float initRotation;
		[HideInInspector]
		public bool initFacingRight;
		[HideInInspector]
		public float toPlayerX;
		public float moveMultiplier = 1;
		public MonoBehaviour[] enableWhileAlive = new MonoBehaviour[0];
		public DeathSound deathSoundPrefab;
		public AudioClip[] deathAudioClips = new AudioClip[0];

		public override void Start ()
		{
			// base.Start ();
			velocityEffectors_floatDict.Clear();
			foreach (VelocityEffector_float velocityEffector_float in velocityEffectors_float)
				velocityEffectors_floatDict.Add(velocityEffector_float.name, velocityEffector_float);
			velocityEffectors_Vector2Dict.Clear();
			foreach (VelocityEffector_Vector2 velocityEffector_Vector2 in velocityEffectors_Vector2)
				velocityEffectors_Vector2Dict.Add(velocityEffector_Vector2.name, velocityEffector_Vector2);
			whatICollideWith = Physics2D.GetLayerCollisionMask(gameObject.layer);
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (healthbarTrs == null)
					healthbarTrs = GetComponent<Transform>().Find("HP Canvas (World)").GetChild(1);
				if (battleIAmPartOf == null && World.Instance != null)
				{
					foreach (EnemyBattle enemyBattle in World.Instance.enemyBattles)
					{
						if (enemyBattle.enemies.Contains(this))
						{
							battleIAmPartOf = enemyBattle;
							break;
						}
					}
				}
				Init ();
				return;
			}
#endif
			hp = maxHp;
			rigid.centerOfMass = trs.InverseTransformPoint(collider.bounds.center);
		}

		public override void DoUpdate ()
		{
			if (GameManager.paused || isDead)// || this == null)
				return;
			base.DoUpdate ();
			if (flipToPlayerX_Trs != null)
			{
				toPlayerX = Player.instance.trs.position.x - trs.position.x;
				if (MathfExtensions.Sign(toPlayerX) != MathfExtensions.Sign(previousToPlayerX))
				{
					flipToPlayerX_Trs.localScale = flipToPlayerX_Trs.localScale.SetX(Mathf.Sign(toPlayerX));
					previousToPlayerX = toPlayerX;
				}
			}
		}

		public virtual void Init ()
		{
			initPosition = trs.position;
			initRotation = trs.eulerAngles.z;
			if (flipToPlayerX_Trs != null)
				initFacingRight = flipToPlayerX_Trs.localScale.x > 0;
		}

		public virtual void Reset ()
		{
			StopAllCoroutines();
			trs.position = initPosition;
			trs.eulerAngles = Vector3.forward * initRotation;
			if (flipToPlayerX_Trs != null)
				flipToPlayerX_Trs.localScale = flipToPlayerX_Trs.localScale.SetX(initFacingRight.PositiveOrNegative());
			previousToPlayerX = 0;
			rigid.velocity = Vector2.zero;
			for (int i = 0; i < velocityEffectors_Vector2Dict.Count; i ++)
				velocityEffectors_Vector2Dict[velocityEffectors_Vector2Dict.Keys.Get(i)].effect = Vector2.zero;
			rigid.angularVelocity = 0;
			hp = maxHp;
			healthbarTrs.localScale = healthbarTrs.localScale.SetX(1);
			healthbarTrs.parent.gameObject.SetActive(true);
			if (shooter != null)
			{
				shooter.shootTimer.Reset ();
				shooter.shootTimer.timeRemaining = 0;
			}
			if (!gameObject.activeSelf)
				gameObject.SetActive(true);
			else
				GameManager.updatables = GameManager.updatables.Add(this);
			if (animator != null)
			{
				animator.ResetTrigger(deathAnimTriggerName);
				animator.ResetTrigger(moveAnimTriggerName);
				animator.ResetTrigger(attackAnimTriggerName);
				animator.SetTrigger(idleAnimTriggerName);
				animator.Play("Idle");
				animator.SetLayerWeight(1, 1);
			}
			if (spriteSkin != null)
				spriteSkin.enabled = false;
			rigid.gravityScale = 0;
			rigid.freezeRotation = true;
			activateOnDeath.SetActive(false);
			collider.enabled = true;
			rigid.centerOfMass = trs.InverseTransformPoint(collider.bounds.center);
			isDead = false;
			for (int i = 0; i < enableWhileAlive.Length; i ++)
			{
				MonoBehaviour _enableWhileAlive = enableWhileAlive[i];
				_enableWhileAlive.enabled = true;
			}
		}

		public override void HandleVelocity ()
		{
			Move ((Vector2) (Player.instance.trs.position - trs.position));
			base.HandleVelocity ();
		}

		public override void Move (Vector2 move)
		{
			base.Move (move);
			velocityEffectors_Vector2Dict["Movement"].effect *= moveMultiplier;
		}

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			base.OnCollisionEnter2D (coll);
			Arrow arrow = coll.gameObject.GetComponent<Arrow>();
			if (arrow != null)
			{
				float damage = Mathf.Sqrt(arrow.speedSqr) * arrow.damageMultiplier;
				TakeDamage (damage);
				Blood blood = ObjectPool.instance.SpawnComponent<Blood>(bloodPrefab.prefabIndex, coll.GetContact(0).point, Quaternion.LookRotation(Vector3.forward, arrow.trs.up));
				blood.particleSystem.emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, damage) });
				blood.particleSystem.Play();
			}
			else
			{
				if (coll.gameObject == Player.instance.gameObject)
				{
					float damage = Mathf.Sqrt(Player.instance.speedSqr) - Player.instance.velocityEffectors_floatDict["Move Speed"].effect;
					if (damage <= 0)
						return;
					damage *= Player.instance.damageMultiplierWhenOntopEnemies;
					TakeDamage (damage);
					Blood blood = ObjectPool.instance.SpawnComponent<Blood>(bloodPrefab.prefabIndex, coll.GetContact(0).point, Quaternion.LookRotation(Vector3.forward, coll.GetContact(0).normal));
					blood.particleSystem.emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, damage) });
					blood.particleSystem.Play();
				}
			}
			// OnCollisionStay2D (coll);
		}

		// public virtual void OnCollisionStay2D (Collision2D coll)
		// {
		// 	Player player = coll.gameObject.GetComponent<Player>();
		// 	if (player != null)
		// 		player.TakeDamage ();
		// }

		public virtual void TakeDamage (float damage)
		{
			hp -= damage;
			healthbarTrs.localScale = healthbarTrs.localScale.SetX(hp / maxHp);
			if (hp <= 0)
				Death ();
		}

		public virtual void Death ()
		{
			isDead = true;
			GameManager.updatables = GameManager.updatables.Remove(this);
			for (int i = 0; i < enableWhileAlive.Length; i ++)
			{
				MonoBehaviour _enableWhileAlive = enableWhileAlive[i];
				_enableWhileAlive.enabled = false;
			}
			Arrow[] arrowsStuckToMe = GetComponentsInChildren<Arrow>();
			foreach (Arrow arrowStuckToMe in arrowsStuckToMe)
			{
				if (arrowStuckToMe.GetType() != typeof(WindArrow))
				{
					arrowStuckToMe.collider.isTrigger = false;
					arrowStuckToMe.rigid.bodyType = RigidbodyType2D.Dynamic;
					arrowStuckToMe.trs.SetParent(null);
					GameManager.updatables = GameManager.updatables.Add(arrowStuckToMe);
				}
			}
			if (onDeath != null)
				onDeath ();
			if (shooter != null)
				shooter.shootTimer.Stop ();
			if (animator != null)
			{
				animator.ResetTrigger(attackAnimTriggerName);
				animator.ResetTrigger(idleAnimTriggerName);
				animator.ResetTrigger(moveAnimTriggerName);
				animator.SetTrigger(deathAnimTriggerName);
				animator.SetLayerWeight(1, 0);
			}
			activateOnDeath.SetActive(true);
			collider.enabled = false;
			rigid.gravityScale = velocityEffectors_floatDict["Gravity Scale"].effect;
			rigid.freezeRotation = false;
			rigid.centerOfMass = trs.InverseTransformPoint(deadCollider.bounds.center);
			healthbarTrs.parent.gameObject.SetActive(false);
			if (deathSoundPrefab != null)
				AudioManager.Instance.PlaySoundEffect (deathSoundPrefab, deathAudioClips[Random.Range(0, deathAudioClips.Length)]);
			StopAllCoroutines();
			StartCoroutine(DecayRoutine ());
		}

		public virtual IEnumerator DecayRoutine ()
		{
			yield return new WaitForSeconds(decayDelay);
			gameObject.SetActive(false);
		}

		public virtual void SetOnDeathListener (OnDeath onDeath)
		{
			this.onDeath = onDeath;
		}

		public override void OnDisable ()
		{
			base.OnDisable ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

// 		public virtual void OnDestroy ()
// 		{
// #if UNITY_EDITOR
// 			if (!Application.isPlaying)
// 				return;
// #endif
// 			onDeath = null;
// 		}
	}
}