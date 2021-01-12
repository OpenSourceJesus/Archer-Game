using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;
using DialogAndStory;
using System.Collections;

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class Player : PlatformerEntity, IDestructable, ISaveableAndLoadable
	{
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		public static Player instance;
		public static float timeOfLastShot;
		public static int addToMoneyOnSave;
		public ArrowEntry[] arrowEntries;
		[HideInInspector]
		public int currentArrowEntryIndex;
		public ArrowEntry CurrentArrowEntry
		{
			get
			{
				return arrowEntries[currentArrowEntryIndex];
			}
			set
			{
				for (int i = 0; i < arrowEntries.Length; i ++)
				{
					if (arrowEntries[i].arrowPrefab == value.arrowPrefab)
					{
						currentArrowEntryIndex = i;
						break;
					}
				}
			}
		}
		public Timer reloadTimer;
		public float shootSpeed;
		[HideInInspector]
		public bool canShoot = true;
		Vector2 aimVector;
		public float arrowLifetime;
		float hp;
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
		public CircularMenu arrowMenu;
		public LineRenderer aimingVisualizer;
		public LayerMask whatArrowsCollideWith;
		[HideInInspector]
		public bool canTeleport;
		public AudioClip[] shootAudioClips = new AudioClip[0];
		bool isUnderwater;
		[HideInInspector]
		public bool isHurting;
		public GameObject lifeIcon;
		public Transform lifeIconsParent;
		// [HideInInspector]
		[SaveAndLoadValue(false)]
		public Vector2 spawnPosition;
		[HideInInspector]
		[SerializeField]
		float arrowHeight;
		[HideInInspector]
		[SerializeField]
		float arrowDrag;
#if UNITY_EDITOR
		public bool ownAllArrows;
#endif
		public Transform aimerTrs;
		public Transform shootSpawnPoint;
		public TMP_Text moneyText;
		public TemporaryActiveObject moneyChangedIndicator;
		public TMP_Text moneyChangedAmountText;
		[HideInInspector]
		public float speedSqr;
		public float damageMultiplierWhenOntopEnemies;
		Dictionary<Collider2D, Vector2[]> collisionNormalsDict = new Dictionary<Collider2D, Vector2[]>();
		public float crushingAngle;
		public int playerIndex;
		public Blood bloodPrefab;
		public Collider2D arrowSensor;
		public List<Collider2D> shotArrowColliders = new List<Collider2D>();
		public List<Collider2D> shotArrowCollidersThatLeftBody = new List<Collider2D>();
		public Transform alternateShootSpawnPoint;

		public override void Start ()
		{
			base.Start ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorPrefs.SetFloat("Arrow size.x", GetArrowEntry(typeof(Arrow)).arrowPrefab.collider.GetSize().x);
				EditorPrefs.SetFloat("Arrow size.y", GetArrowEntry(typeof(Arrow)).arrowPrefab.collider.GetSize().y);
				EditorPrefs.SetFloat("Arrow drag", GetArrowEntry(typeof(Arrow)).arrowPrefab.rigid.drag);
				Init ();
				return;
			}
			else
			{
				if (ownAllArrows)
				{
					for (int i = 0; i < arrowEntries.Length; i ++)
					{
						ArrowEntry arrowEntry = arrowEntries[i];
						arrowEntry.isOwned = true;
						arrowEntry.menuOptionToSwitchToMe.SetActive(true);
						arrowEntries[i] = arrowEntry;
					}
				}
			}
#endif
			Init ();
			hp = maxHp;
			if (lifeIconsParent != null)
			{
				for (int i2 = 1; i2 < maxHp; i2 ++)
					Instantiate(lifeIcon, lifeIconsParent);
				for (int i = 0; i < arrowEntries.Length; i ++)
				{
					ArrowEntry arrowEntry = arrowEntries[i];
					arrowMenu.options[i].button.onClick.AddListener(delegate { SwitchArrowType (arrowEntry); });
					arrowEntry.menuOptionToSwitchToMe.SetActive(arrowEntry.isOwned);
					arrowEntries[i] = arrowEntry;
				}
			}
		}

		public virtual void Init ()
		{
			Arrow arrowPrefab = GetArrowEntry(typeof(Arrow)).arrowPrefab;
			// float arrowWidth = arrowPrefab.collider.GetSize().x;
			// arrowHeight = arrowPrefab.collider.GetSize().y;
			float arrowWidth = 0.1175385f;
			arrowHeight = 0.8387692f;
			arrowDrag = arrowPrefab.rigid.drag;
#if UNITY_EDITOR
			arrowWidth = EditorPrefs.GetFloat("Arrow size.x", arrowWidth);
			arrowHeight = EditorPrefs.GetFloat("Arrow size.y", arrowHeight);
			arrowDrag = EditorPrefs.GetFloat("Arrow drag", arrowDrag);
#endif
			aimingVisualizer.startWidth = arrowWidth;
			aimingVisualizer.endWidth = aimingVisualizer.startWidth;
			if (spawnPosition == Vector2.zero)
				spawnPosition = trs.position;
			instance = this;
		}

		public virtual void SwitchArrowType (ArrowEntry arrowEntry)
		{
			if (arrowEntry.isOwned)
			{
				Conversation tutorialConversation = null;
				if (ArrowCollectible.tutorialConversationsDict.TryGetValue(arrowEntry.arrowPrefab.GetType(), out tutorialConversation) && tutorialConversation != null && tutorialConversation.lastStartedDialog != null)
					DialogManager.Instance.EndDialog (ArrowCollectible.tutorialConversationsDict[arrowEntry.arrowPrefab.GetType()].lastStartedDialog);
				CurrentArrowEntry = arrowEntry;
			}
		}

		public override void DoUpdate ()
		{
			if (PauseMenu.Instance.gameObject.activeSelf || AccountSelectMenu.Instance.gameObject.activeSelf)
				return;
			base.DoUpdate ();
			HandleAiming ();
			HandleShooting ();
			HandleSwitchArrowType ();
			HandleArrowReturning ();
			HandleArrowReturnNotifications ();
			speedSqr = rigid.velocity.sqrMagnitude;
		}

		int switchArrowInput;
		int preivousSwitchArrowInput;
		void HandleSwitchArrowType ()
		{
			switchArrowInput = InputManager.GetSwitchArrowInput(playerIndex);
			if (switchArrowInput != -1 && preivousSwitchArrowInput != switchArrowInput)
				SwitchArrowType (arrowEntries[switchArrowInput]);
			preivousSwitchArrowInput = switchArrowInput;
		}
		
		Vector2 currentPosition;
		Vector2 currentVelocity;
		float currentGravityScale;
		float currentDrag;
		List<Vector3> positions = new List<Vector3>();
		bool wasPreviouslyInSpiderWeb;
		RaycastHit2D hit;
		RaycastHit2D[] hits = new RaycastHit2D[1];
		bool wasPreviouslyInWater;
		public virtual void HandleAiming ()
		{
			aimVector = InputManager.GetAimInput(playerIndex);
			currentGravityScale = 1;
			currentDrag = arrowDrag;
			currentPosition = shootSpawnPoint.position;
			currentVelocity = (aimVector * shootSpeed) + rigid.velocity;
			positions.Clear();
			positions.Add(currentPosition);
			wasPreviouslyInSpiderWeb = false;
			wasPreviouslyInWater = false;
			do
			{
				if (!GameManager.gameModifierDict["Unslowable Pull Arrows"].isActive || CurrentArrowEntry.arrowPrefab.GetType() != typeof(PullArrow))
				{
					contactFilter = new ContactFilter2D();
					contactFilter.useTriggers = true;
					contactFilter.layerMask = LayerMask.GetMask("Spider Web");
					contactFilter.useLayerMask = true;
					if (!wasPreviouslyInSpiderWeb && Physics2DExtensions.LinecastWithWidth(currentPosition - currentVelocity.normalized * arrowHeight / 2, currentPosition + currentVelocity.normalized * arrowHeight / 2, aimingVisualizer.startWidth, contactFilter, hits) > 0)
					{
						currentPosition = hits[0].point;
						currentVelocity = Vector2.zero;
						wasPreviouslyInSpiderWeb = true;
					}
					else
					{
						contactFilter = new ContactFilter2D();
						contactFilter.useTriggers = true;
						contactFilter.layerMask = LayerMask.GetMask("Water");
						contactFilter.useLayerMask = true;
						if (Physics2DExtensions.LinecastWithWidth(currentPosition - currentVelocity.normalized * arrowHeight / 2, currentPosition + currentVelocity.normalized * arrowHeight / 2, aimingVisualizer.startWidth, contactFilter, hits) > 0)
						{
							if (!wasPreviouslyInWater)
							{
								currentDrag += Water.Instance.addToLinearDrag;
								currentGravityScale -= Water.instance.subtractFromGravityScale;
								wasPreviouslyInWater = true;
							}
						}
						else if (wasPreviouslyInWater)
						{
							currentDrag -= Water.Instance.addToLinearDrag;
							currentGravityScale += Water.instance.subtractFromGravityScale;
							wasPreviouslyInWater = false;
						}
					}
				}
				currentVelocity += Physics2D.gravity * currentGravityScale * Time.fixedDeltaTime;
				currentVelocity *= (1f - Time.fixedDeltaTime * currentDrag);
				currentPosition += currentVelocity * Time.fixedDeltaTime;
				hit = Physics2DExtensions.LinecastWithWidth(positions[positions.Count - 1], currentPosition, aimingVisualizer.startWidth, whatArrowsCollideWith);
				if (hit.collider != null)
				{
					positions.Add(hit.point);
					break;
				}
				if (positions.Contains(currentPosition))
					break;
				positions.Add(currentPosition);
			} while (GameCamera.Instance.viewRect.Contains(currentPosition));
			aimingVisualizer.positionCount = positions.Count;
			aimingVisualizer.SetPositions(positions.ToArray());
			if (aimVector != Vector2.zero)
				aimerTrs.right = aimVector * trs.localScale.x;
			if (Mathf.Sign(aimVector.x) != trs.localScale.x)
				trs.localScale = trs.localScale.SetX(trs.localScale.x * -1);
		}

		bool shootInput;
		bool previousShootInput;
		public virtual void HandleShooting ()
		{
			shootInput = InputManager.GetShootInput(playerIndex);
			if (MachineLearningManager.Instance.isLearning)
				shootInput = Input.GetKey(KeyCode.Mouse0);
			if (shootInput && reloadTimer.timeRemaining <= 0 && canShoot && CurrentArrowEntry.holdingCount > 0 && CurrentArrowEntry.isOwned && CurrentArrowEntry.canShoot)
				Shoot (aimVector);
			else if (!shootInput && previousShootInput)
				CurrentArrowEntry.canShoot = true;
			previousShootInput = shootInput;
		}

		public virtual void HandleArrowReturnNotifications ()
		{
			foreach (ArrowEntry arrowEntry in arrowEntries)
			{
				if (arrowEntry.isOwned && arrowEntry.holdingCount == 0)
				{
					arrowEntry.returnNotification.gameObject.SetActive(true);
					ObjectPool.DelayedDespawn delayedDespawn = arrowEntry.arrows[0].delayedDespawn;
					arrowEntry.returnNotification.fillAmount = delayedDespawn.timeRemaining / delayedDespawn.duration;
				}
				else
					arrowEntry.returnNotification.gameObject.SetActive(false);
			}
		}

		public virtual Arrow Shoot (Vector2 shoot)
		{
			timeOfLastShot = Time.time;
			Arrow arrow;
			if (Physics2D.OverlapPoint(shootSpawnPoint.position, whatArrowsCollideWith) == null)
				arrow = ObjectPool.Instance.SpawnComponent<Arrow>(CurrentArrowEntry.arrowPrefab.prefabIndex, shootSpawnPoint.position, Quaternion.LookRotation(Vector3.forward, aimVector));
			else
				arrow = ObjectPool.Instance.SpawnComponent<Arrow>(CurrentArrowEntry.arrowPrefab.prefabIndex, alternateShootSpawnPoint.position, Quaternion.LookRotation(Vector3.forward, aimVector));
			arrow.owner = this;
			arrow.rigid.velocity = aimVector * shootSpeed + rigid.velocity;
			arrow.speedSqr = arrow.rigid.velocity.magnitude;
			arrow.delayedDespawn = ObjectPool.instance.DelayDespawn (arrow.prefabIndex, arrow.gameObject, arrow.trs, arrowLifetime);
			CurrentArrowEntry.holdingCount --;
			CurrentArrowEntry.arrows = CurrentArrowEntry.arrows.Add(arrow);
			shotArrowColliders.Add(arrow.collider);
			reloadTimer.Reset ();
			reloadTimer.Start ();
			AudioManager.Instance.PlaySoundEffect (new SoundEffect.Settings(shootAudioClips[Random.Range(0, shootAudioClips.Length)]));
			if (GameManager.Instance.movementJumpingShootingTutorialConversation != null && GameManager.instance.movementJumpingShootingTutorialConversation.lastStartedDialog == GameManager.instance.shootingTutorialDialog)
				GameManager.instance.DeactivateGoForever (GameManager.instance.movementJumpingShootingTutorialConversation.gameObject);
			return arrow;
		}

		public override void HandleVelocity ()
		{
			if (canMoveAndJump)
			{
				if (!MachineLearningManager.Instance.isLearning)
				{
					if (InputManager.UsingGamepad || isSwimming)
						Move (InputManager.GetSwimInput(playerIndex));
					else
						Move (Vector2.right * InputManager.GetMoveInput(playerIndex));
				}
				else
				{
					// if (InputManager.UsingGamepad || isSwimming)
					// 	Move (InputManager.GetSwimInput(playerIndex));
					// else
					// {
						int move = 0;
						if (Input.GetKey(KeyCode.D))
							move ++;
						if (Input.GetKey(KeyCode.A))
							move --;
						Move (Vector2.right * move);
					// }
				}
			}
			base.HandleVelocity ();
		}

		public override void Move (Vector2 move)
		{
			// if (!isSwimming && move.x != 0)
			// 	trs.localScale = trs.localScale.SetX(Mathf.Sign(move.x));
			base.Move (move);
			if (move.x != 0)
				DialogManager.Instance.EndDialog (GameManager.Instance.movementTutorialDialog);
		}

		public override void HandleJumping ()
		{
			if (!canMoveAndJump)
				return;
			bool jumpInput = InputManager.GetJumpInput(playerIndex);
			if (MachineLearningManager.Instance.isLearning)
				jumpInput = Input.GetKey(KeyCode.W);
			if (jumpInput && isGrounded && !isJumping)
				StartJump ();
			else if (isJumping)
			{
				if (!jumpInput)
				{
					StopJump ();
					if (GameManager.Instance.movementJumpingShootingTutorialConversation != null && GameManager.instance.movementJumpingShootingTutorialConversation.lastStartedDialog == GameManager.instance.jumpingTutorialDialog)
						DialogManager.Instance.EndDialog (GameManager.instance.jumpingTutorialDialog);
				}
				if (rigid.velocity.y <= 0)
				{
					isJumping = false;
					if (GameManager.Instance.movementJumpingShootingTutorialConversation != null && GameManager.instance.movementJumpingShootingTutorialConversation.lastStartedDialog == GameManager.instance.jumpingTutorialDialog)
						DialogManager.Instance.EndDialog (GameManager.instance.jumpingTutorialDialog);
				}
			}
		}

		// public override void StartJump ()
		// {
		// 	base.StartJump ();
		// 	bottomLeftColliderCorner = ColliderRect.min;
		// 	bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
		// 	Vector2 offsetPhysicsQuery = Vector2.down * groundLinecastOffset.positionChangeAlongPerpendicular;
		// 	RaycastHit2D hit = Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), LayerMask.GetMask("Enemy"));
		// 	if (hit.collider != null)
		// 	{
		// 		Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
		// 		float damage = Mathf.Sqrt(speedSqr) - velocityEffectors_floatDict["Move Speed"].effect;
		// 		if (damage <= 0)
		// 			return;
		// 		damage *= damageMultiplierWhenOntopEnemies;
		// 		enemy.TakeDamage (damage);
		// 		Blood blood = ObjectPool.instance.SpawnComponent<Blood>(enemy.bloodPrefab.prefabIndex, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
		// 		blood.particleSystem.emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, damage) });
		// 		blood.particleSystem.Play();
		// 	}
		// }

		public override void HandleSwimming ()
		{
			base.HandleSwimming ();
			if (waterRectIAmIn != RectExtensions.NULL && IsUnderwater(waterRectIAmIn))
			{
				if (!isUnderwater)
				{
					animator.Play("Underwater", 1);
					isUnderwater = true;
				}
				else if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Underwater"))
					TakeDamage ();
			}
			else
			{
				animator.Play("None", 1);
				isUnderwater = false;
			}
		}

		public override void StopSwimming ()
		{
			base.StopSwimming ();
			isUnderwater = false;
		}

		public virtual void TakeDamage (float damage = 1)
		{
			if (isHurting)
				return;
			isHurting = true;
			GameManager.Instance.screenEffectAnimator.Play("Hurt");
			hp --;
			if (lifeIconsParent != null)
				Destroy(lifeIconsParent.GetChild(0).gameObject);
			if (hp <= 0)
				Death ();
		}

		public virtual void FullHeal ()
		{
			for (int i = (int) hp; i < maxHp; i ++)
				Instantiate(lifeIconsParent.GetChild(0), lifeIconsParent);
			hp = maxHp;
		}

		public virtual void Death ()
		{
			addToMoneyOnSave = 0;
			if (QuestManager.currentQuest != null)
				GameManager.onGameScenesLoaded += delegate { QuestManager.Instance.ShowRetryScreen (); };
			GameManager.Instance.LoadGameScenes ();
		}

		public virtual int GetArrowEntryIndex (Type arrowType)
		{
			for (int i = 0; i < arrowEntries.Length; i ++)
			{
				if (arrowType.Name == arrowEntries[i].arrowPrefab.GetType().Name)
					return i;
			}
			return -1;
		}

		public virtual ArrowEntry GetArrowEntry (Type arrowType)
		{
			return arrowEntries[GetArrowEntryIndex(arrowType)];
		}

		public virtual void HandleArrowReturning ()
		{
			while (true)
			{
				int indexOfNull = shotArrowCollidersThatLeftBody.IndexOf(null);
				if (indexOfNull == -1)
					break;
				else
					shotArrowCollidersThatLeftBody.RemoveAt(indexOfNull);
			}
			ContactFilter2D contactFilter = new ContactFilter2D();
			contactFilter.useLayerMask = true;
			contactFilter.layerMask = LayerMask.GetMask("Arrow");
			List<Collider2D> hitArrowColliders = new List<Collider2D>();
			int hitCount = Physics2D.OverlapCollider(arrowSensor, contactFilter, hitArrowColliders);
			if (hitCount == 0)
			{
				for (int i = 0; i < shotArrowColliders.Count; i ++)
				{
					Collider2D shotArrowCollider = shotArrowColliders[i];
					if (shotArrowCollider != null)
						shotArrowCollidersThatLeftBody.Add(shotArrowCollider);
				}
				shotArrowColliders.Clear();
			}
			else
			{
				for (int i = 0; i < hitArrowColliders.Count; i ++)
				{
					Collider2D hitArrowCollider = hitArrowColliders[i];
					int indexOfArrow = shotArrowCollidersThatLeftBody.IndexOf(hitArrowCollider);
					if (indexOfArrow != -1)
					{
						shotArrowCollidersThatLeftBody.RemoveAt(indexOfArrow);
						Arrow arrow = hitArrowCollider.GetComponent<Arrow>();
						if (arrow.owner == this)
							Destroy(arrow.gameObject);
						else if (!arrow.collider.isTrigger)
						{
							float damage = Mathf.Sqrt(arrow.speedSqr) * arrow.damageMultiplier;
							TakeDamage (damage);
							Blood blood = ObjectPool.instance.SpawnComponent<Blood>(bloodPrefab.prefabIndex, arrow.trs.position, Quaternion.LookRotation(Vector3.forward, arrow.trs.up));
							blood.particleSystem.emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, damage) });
							blood.particleSystem.Play();
							GameManager.updatables = GameManager.updatables.Remove(arrow);
							arrow.rigid.bodyType = RigidbodyType2D.Kinematic;
							arrow.rigid.velocity = Vector2.zero;
							arrow.collider.isTrigger = true;
							arrow.trs.SetParent(trs);
						}
						i --;
					}
				}
			}
			
		}

		// public virtual void OnTriggerEnter2D (Collider2D other)
		// {
		// 	OnTriggerStay2D (other);
		// }

		// public virtual void OnTriggerStay2D (Collider2D other)
		// {
		// 	// print(other.name);
		// 	if (arrowHasLeftBody && Time.time - timeOfLastShot >= Time.deltaTime * 2)
		// 	{
		// 		Arrow arrow = other.GetComponent<Arrow>();
		// 		if (arrow != null)
		// 		{
		// 			if (arrow.owner == this)
		// 				Destroy(other.gameObject);
		// 			else if (!arrow.collider.isTrigger)
		// 			{
		// 				float damage = Mathf.Sqrt(arrow.speedSqr) * arrow.damageMultiplier;
		// 				TakeDamage (damage);
		// 				Blood blood = ObjectPool.instance.SpawnComponent<Blood>(bloodPrefab.prefabIndex, arrow.trs.position, Quaternion.LookRotation(Vector3.forward, arrow.trs.up));
		// 				blood.particleSystem.emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, damage) });
		// 				blood.particleSystem.Play();
		// 				GameManager.updatables = GameManager.updatables.Remove(arrow);
		// 				arrow.rigid.bodyType = RigidbodyType2D.Kinematic;
		// 				arrow.rigid.velocity = Vector2.zero;
		// 				arrow.collider.isTrigger = true;
		// 				arrow.trs.SetParent(trs);
		// 			}
		// 		}
		// 		// else
		// 		// {
		// 		// 	print("No arrow");
		// 		// }
		// 	}
		// 	// else
		// 	// {
		// 	// 	print(nameof(arrowHasLeftBody) + " " + arrowHasLeftBody);
		// 	// 	print(Time.time - timeOfLastShot >= Time.deltaTime * 2);
		// 	// }
		// }

		// public virtual void OnTriggerExit2D (Collider2D other)
		// {
		// 	if (other.GetComponent<Arrow>() != null)
		// 		arrowHasLeftBody = true;
		// }

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			OnCollisionStay2D (coll);
			base.OnCollisionEnter2D (coll);
		}

		public virtual void OnCollisionStay2D (Collision2D coll)
		{
			if (coll.gameObject.GetComponent<Enemy>() != null)
				return;
			Vector2 normal;
			Vector2[] normals = new Vector2[coll.contactCount];
			for (int i = 0; i < coll.contactCount; i ++)
			{
				normal = coll.GetContact(i).normal;
				foreach (KeyValuePair<Collider2D, Vector2[]> keyValuePair in collisionNormalsDict)
				{
					if (keyValuePair.Key != coll.collider)
					{
						for (int i2 = 0; i2 < keyValuePair.Value.Length; i2 ++)
						{
							Vector2 collisionNormal = keyValuePair.Value[i2];
							if (Vector2.Angle(normal, collisionNormal) >= crushingAngle)
							{
								bool shouldDie = true;
								MoveTile moveTile = coll.gameObject.GetComponent<MoveTile>();
								if (moveTile != null)
									shouldDie = !moveTile.switchDirectionsWhenHit || Vector2.Angle((Vector2) moveTile.trs.position - moveTile.previousPosition, normal) >= crushingAngle;
								if (shouldDie)
								{
									print("Crushed");
									Death ();
									return;
								}
							}
						}
					}
				}
				normals[i] = normal;
			}
			if (collisionNormalsDict.ContainsKey(coll.collider))
				collisionNormalsDict[coll.collider] = normals;
			else
				collisionNormalsDict.Add(coll.collider, normals);
		}

		public virtual void OnCollisionExit2D (Collision2D coll)
		{
			collisionNormalsDict.Remove(coll.collider);
		}

		public virtual void AddMoney (int amount)
		{
			ArchivesManager.CurrentlyPlaying.CurrentMoney += amount;
			ArchivesManager.CurrentlyPlaying.TotalMoney += amount;
			moneyText.text = "" + ArchivesManager.CurrentlyPlaying.CurrentMoney;
		}

		public void DisplayMoney ()
		{
			moneyChangedAmountText.text = "" + (ArchivesManager.CurrentlyPlaying.CurrentMoney + addToMoneyOnSave);
			moneyChangedIndicator.Do ();
		}

		public override void SetIsGrounded ()
		{
			bottomLeftColliderCorner = ColliderRect.min;
			bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
			Vector2 offsetPhysicsQuery = Vector2.down * groundLinecastOffset.positionChangeAlongPerpendicular;
			RaycastHit2D[] hits = new RaycastHit2D[2];
			contactFilter = new ContactFilter2D();
			contactFilter.layerMask = whatICollideWith;
			contactFilter.useLayerMask = true;
			isGrounded = Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), contactFilter, hits) > 0;
			if (isGrounded)
			{
				for (int i = 0; i < hits.Length; i ++)
				{
					RaycastHit2D hit = hits[i];
					if (hit.collider != null)
					{
						ToggleTile toggleTile = hit.collider.GetComponent<ToggleTile>();
						if (toggleTile != null)
						{
							toggleTile.Toggle ();
							break;
						}
					}
				}
				rigid.gravityScale = 1;
			}
			else
				rigid.gravityScale = 0;
		}

		[Serializable]
		public class ArrowEntry
		{
			public bool isOwned;
			public bool canShoot;
			public string keyToSwitchToMe;
			public GameObject menuOptionToSwitchToMe;
			public Arrow arrowPrefab;
			public int holdingCount;
			[HideInInspector]
			public Arrow[] arrows = new Arrow[0];
			public Image returnNotification;
		}
	}
}