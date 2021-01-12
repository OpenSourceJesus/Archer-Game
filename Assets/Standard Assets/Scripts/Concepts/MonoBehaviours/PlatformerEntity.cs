using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class PlatformerEntity : MonoBehaviour, IUpdatable
	{
		public Animator animator;
		public Transform trs;
		public new Collider2D collider;
		public Rigidbody2D rigid;
		public float jumpSpeed;
		[HideInInspector]
		public bool isGrounded;
		[HideInInspector]
		public bool isJumping;
		public virtual bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		[HideInInspector]
		public LayerMask whatICollideWith;
		// public const float HILL_CHECK_DISTANCE = 1.2f;
		public Rect ColliderRect
		{
			get
			{
				// Rect output = collider.bounds.ToRect();
				// BoxCollider2D boxCollider = collider as BoxCollider2D;
				// if (boxCollider != null)
				// 	output = output.Expand(Vector2.one * boxCollider.edgeRadius * 2);
				// return output.SetPositiveSize();
				return collider.GetRect(trs);
			}
		}
		[HideInInspector]
		public Rect unrotatedColliderRectOnOrigin;
		public Rect UnrotatedColliderRect
		{
			get
			{
				return unrotatedColliderRectOnOrigin.Move(trs.position);
			}
		}
		public bool canSwim;
		[HideInInspector]
		public bool isSwimming;
		protected Rect waterRectIAmIn = RectExtensions.NULL;
		protected Vector2 bottomLeftColliderCorner;
		protected Vector2 bottomRightColliderCorner;
		protected Vector2 topLeftColliderCorner;
		protected Vector2 topRightColliderCorner;
		[HideInInspector]
		public bool canMoveAndJump = true;
		protected bool isHittingWall;
		public VelocityEffector_float[] velocityEffectors_float = new VelocityEffector_float[0];
		public Dictionary<string, VelocityEffector_float> velocityEffectors_floatDict = new Dictionary<string, VelocityEffector_float>();
		public VelocityEffector_Vector2[] velocityEffectors_Vector2 = new VelocityEffector_Vector2[0];
		public Dictionary<string, VelocityEffector_Vector2> velocityEffectors_Vector2Dict = new Dictionary<string, VelocityEffector_Vector2>();
		protected ContactFilter2D contactFilter;
		// protected bool previousIsGrounded;
		// public LayerMask whatIPush;
		RaycastHit2D hit;
		public ObjectInWorld worldObject;
		protected Vector2 previousVelocity;
		public LinecastOffset groundLinecastOffset;
		public LinecastOffset wallLinecastOffset;
		public LinecastOffset roofLinecastOffset;
		[HideInInspector]
		public float previousXPosition;
		public float maxXVelocityToBeStuck;
		public float raiseAmountToGetUnstuck;
		public string idleAnimTriggerName = "toEXIT";
		public string moveAnimTriggerName = "toWALK";
		public string jumpAnimTriggerName = "toJUMP";

		public virtual void Start ()
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
				if (worldObject == null)
					worldObject = GetComponent<ObjectInWorld>();
				whatICollideWith = Physics2D.GetLayerCollisionMask(gameObject.layer);
				unrotatedColliderRectOnOrigin.size = collider.GetSize(trs);
				unrotatedColliderRectOnOrigin.center = Vector2.zero;
				return;
			}
#endif
			velocityEffectors_floatDict.Clear();
			foreach (VelocityEffector_float velocityEffector_float in velocityEffectors_float)
				velocityEffectors_floatDict.Add(velocityEffector_float.name, velocityEffector_float);
			velocityEffectors_Vector2Dict.Clear();
			foreach (VelocityEffector_Vector2 velocityEffector_Vector2 in velocityEffectors_Vector2)
				velocityEffectors_Vector2Dict.Add(velocityEffector_Vector2.name, velocityEffector_Vector2);
			whatICollideWith = Physics2D.GetLayerCollisionMask(gameObject.layer);
			previousXPosition = trs.position.x;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void StartSwimming ()
		{
			StartCoroutine(StartSwimmingRoutine ());
		}

		public virtual IEnumerator StartSwimmingRoutine ()
		{
			do
			{
				foreach (Rect waterRect in Water.Instance.waterRects)
				{
					if (IsSwimming(waterRect))
					{
						if (velocityEffectors_Vector2Dict.ContainsKey("Water"))
						{
							if (velocityEffectors_Vector2Dict.ContainsKey("Wind Explosion"))
								velocityEffectors_Vector2Dict["Water"].effect = rigid.velocity - velocityEffectors_Vector2Dict["Wind Explosion"].effect;
							else
								velocityEffectors_Vector2Dict["Water"].effect = rigid.velocity;
						}
						waterRectIAmIn = waterRect;
						isSwimming = true;
						yield break;
					}
				}
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		Rect previousWaterRect;
		public virtual void HandleSwimming ()
		{
			if (waterRectIAmIn != RectExtensions.NULL)
			{
				if (IsSwimming(waterRectIAmIn))
					isSwimming = true;
				else
				{
					previousWaterRect = waterRectIAmIn;
					waterRectIAmIn = RectExtensions.NULL;
					foreach (Rect waterRect in Water.Instance.waterRects)
					{
						if (IsSwimming(waterRect))
						{
							waterRectIAmIn = waterRect;
							isSwimming = true;
							return;
						}
					}
					waterRectIAmIn = previousWaterRect;
				}
			}
		}

		public virtual bool IsSwimming (Rect waterRect)
		{
			return waterRect.yMax >= trs.position.y && ColliderRect.xMax >= waterRect.xMin && ColliderRect.xMin <= waterRect.xMax && waterRect.yMin <= ColliderRect.yMax;
		}

		public virtual bool IsUnderwater (Rect waterRect)
		{
			return waterRect.yMax >= ColliderRect.yMax && ColliderRect.xMax >= waterRect.xMin && ColliderRect.xMin <= waterRect.max.x && waterRect.yMin <= ColliderRect.yMax;
		}

		public virtual void StopSwimming ()
		{
			StopCoroutine(StartSwimmingRoutine ());
			isSwimming = false;
			trs.up = Vector2.up;
			waterRectIAmIn = RectExtensions.NULL;
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DoUpdate ()
		{
			if (GameManager.paused)
				return;
			HandleSwimming ();
			HandleJumping ();
			HandleVelocity ();
			// previousIsGrounded = isGrounded;
		}

		public virtual void SetIsHittingWall ()
		{
			isHittingWall = false;
			if (previousVelocity.x > 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.right * wallLinecastOffset.positionChangeAlongPerpendicular;
				bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
				topRightColliderCorner = ColliderRect.max;
				contactFilter = new ContactFilter2D();
				contactFilter.layerMask = whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.down * wallLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.up * wallLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
				{
					isHittingWall = true;
					foreach (KeyValuePair<string, VelocityEffector_Vector2> keyValuePair in velocityEffectors_Vector2Dict)
						velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x = Mathf.Min(0, velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x);
				}
				// Debug.DrawLine(bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.down * wallLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.up * wallLinecastOffset.lengthChange2), Color.red, 0);
			}
			else if (previousVelocity.x < 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.left * wallLinecastOffset.positionChangeAlongPerpendicular;
				bottomLeftColliderCorner = ColliderRect.min;
				topLeftColliderCorner = new Vector2(ColliderRect.xMin, ColliderRect.yMax);
				contactFilter = new ContactFilter2D();
				contactFilter.layerMask = whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.down * wallLinecastOffset.lengthChange1), topLeftColliderCorner + offsetPhysicsQuery + (Vector2.up * wallLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
				{
					isHittingWall = true;
					foreach (KeyValuePair<string, VelocityEffector_Vector2> keyValuePair in velocityEffectors_Vector2Dict)
						velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x = Mathf.Max(0, velocityEffectors_Vector2Dict[keyValuePair.Key].effect.x);
				}
				// Debug.DrawLine(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.down * wallLinecastOffset.lengthChange1), topLeftColliderCorner + offsetPhysicsQuery + (Vector2.up * wallLinecastOffset.lengthChange2), Color.red, 0);
			}
		}

		public virtual void Move (Vector2 move)
		{
			if (!canMoveAndJump)
				return;
			if (isSwimming)
			{
				move = Vector2.ClampMagnitude(move, 1);
				if (waterRectIAmIn != RectExtensions.NULL && move.sqrMagnitude > 0)
					trs.up = move;
			}
			else
			{
				move.x = Mathf.Clamp(move.x, -1, 1);
				move.y = 0;
			}
			if (animator != null)
			{
				if (!isHittingWall && move.sqrMagnitude > 0)
				{
					animator.ResetTrigger(idleAnimTriggerName);
					animator.SetTrigger(moveAnimTriggerName);
					animator.SetFloat("moveSpeed", move.magnitude);
				}
				else
				{
					animator.ResetTrigger(moveAnimTriggerName);
					// animator.SetTrigger(idleAnimTriggerName);
					animator.SetFloat("moveSpeed", 0);
				}
			}
			velocityEffectors_Vector2Dict["Movement"].effect = move * velocityEffectors_floatDict["Move Speed"].effect;
		}

		public virtual void HandleVelocity ()
		{
			SetIsGrounded ();
			SetIsHittingWall ();
			// if (velocityEffectors_Vector2Dict["Pull Arrow"].effect.sqrMagnitude == 0)
			// {
				SetVelocityEffector (velocityEffectors_Vector2Dict["Falling"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Jump"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Wind Explosion"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Water"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Pull Arrow"]);
				Vector2 velocity = Vector2.zero;
				foreach (VelocityEffector_Vector2 velocityEffector in velocityEffectors_Vector2Dict.Values)
					velocity += velocityEffector.effect;
				rigid.velocity = velocity;
				// rigid.velocity = velocityEffectors_Vector2Dict["Falling"].effect + velocityEffectors_Vector2Dict["Jump"].effect + velocityEffectors_Vector2Dict["Movement"].effect + velocityEffectors_Vector2Dict["Wind Explosion"].effect + velocityEffectors_Vector2Dict["Move Tile"].effect + velocityEffectors_Vector2Dict["Water"].effect;
			// }
			// else
			// {
			// 	velocityEffectors_Vector2Dict["Falling"].effect = Vector2.zero;
			// 	velocityEffectors_Vector2Dict["Jump"].effect = Vector2.zero;
			// 	velocityEffectors_Vector2Dict["Wind Explosion"].effect = Vector2.zero;
			// 	velocityEffectors_Vector2Dict["Water"].effect = Vector2.zero;
			// 	velocityEffectors_Vector2Dict["Movement"].effect = Vector2.zero;
			// 	velocityEffectors_Vector2Dict["Move Tile"].effect = ;
			// 	rigid.velocity = velocityEffectors_Vector2Dict["Pull Arrow"].effect;
			// }
			if (isSwimming)
			{
				if (waterRectIAmIn.yMax <= trs.position.y + rigid.velocity.y * Time.deltaTime)
				{
					if (ColliderRect.xMin + rigid.velocity.x * Time.deltaTime <= waterRectIAmIn.xMin)
					{
						rigid.position = new Vector2(waterRectIAmIn.xMin - Physics2D.defaultContactOffset, waterRectIAmIn.yMax) + new Vector2(-UnrotatedColliderRect.width / 2, UnrotatedColliderRect.height / 2);
						StopSwimming ();
					}
					else if (ColliderRect.xMax + rigid.velocity.x * Time.deltaTime >= waterRectIAmIn.xMax)
					{
						rigid.position = new Vector2(waterRectIAmIn.xMax + Physics2D.defaultContactOffset, waterRectIAmIn.yMax) + new Vector2(UnrotatedColliderRect.width / 2, UnrotatedColliderRect.height / 2);
						StopSwimming ();
					}
					else if (velocityEffectors_Vector2Dict["Pull Arrow"].effect.y == 0 && velocityEffectors_Vector2Dict["Wind Explosion"].effect.y == 0)
					{
						rigid.position = rigid.position.SetY(waterRectIAmIn.yMax);
						rigid.velocity = rigid.velocity.SetY(0);
					}
				}
			}
			previousVelocity = rigid.velocity;
			if (Mathf.Abs(previousVelocity.x) > maxXVelocityToBeStuck && Mathf.Abs(rigid.position.x - previousXPosition) <= maxXVelocityToBeStuck * Time.deltaTime && !isHittingWall)
				rigid.position += Vector2.up * raiseAmountToGetUnstuck;
			previousXPosition = trs.position.x;
		}

		// RaycastHit2D hit2;
		// Vector2 offsetHillCheck;
		public virtual void SetVelocityEffector (VelocityEffector_Vector2 velocityEffector)
		{
			if (velocityEffector.effect.y > 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.up * roofLinecastOffset.positionChangeAlongPerpendicular;
				topLeftColliderCorner = new Vector2(ColliderRect.xMin, ColliderRect.yMax);
				topRightColliderCorner = ColliderRect.max;
				contactFilter = new ContactFilter2D();
				contactFilter.layerMask = whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(topLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * roofLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.right * roofLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
					velocityEffector.effect.y = 0;
				// Debug.DrawLine(topLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * roofLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.right * roofLinecastOffset.lengthChange2), Color.red, 0);
			}
			else if (velocityEffector.effect.y < 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.down * groundLinecastOffset.positionChangeAlongPerpendicular;
				bottomLeftColliderCorner = ColliderRect.min;
				bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
				contactFilter = new ContactFilter2D();
				contactFilter.layerMask = whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
					velocityEffector.effect.y = 0;
				// Debug.DrawLine(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), Color.red, 0);
			}
			float gravityScale = velocityEffectors_floatDict["Gravity Scale"].effect;
			switch (velocityEffector.name)
			{
				// case "Push":
				// 	if (rigid.velocity.x > 0)
				// 	{
				// 		topRightColliderCorner = ColliderRect.max;
				// 		bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
				// 		hit = Physics2D.Linecast(topRightColliderCorner, bottomRightColliderCorner, whatIPush);
				// 		if (hit.collider != null)
				// 		{
				// 			if (hit.rigidbody.velocity.x < 0)
				// 				velocityEffectors_Vector2Dict["Push"].effect.x += hit.rigidbody.velocity.x - hit.rigidbody.mass - hit.rigidbody.drag * Time.deltaTime;
				// 			else
				// 				velocityEffectors_Vector2Dict["Push"].effect.x = Mathf.Clamp(velocityEffectors_Vector2Dict["Push"].effect.x - hit.rigidbody.mass - hit.rigidbody.drag, -Mathf.Infinity, Mathf.Infinity);
				// 		}
				// 		else
				// 			velocityEffectors_Vector2Dict["Push"].effect.x = 0;
				// 	}
				// 	else if (rigid.velocity.x < 0)
				// 	{
				// 		topLeftColliderCorner = new Vector2(ColliderRect.xMin, ColliderRect.xMin);
				// 		bottomLeftColliderCorner = ColliderRect.min;
				// 		hit = Physics2D.Linecast(topLeftColliderCorner, bottomLeftColliderCorner, whatIPush);
				// 		if (hit.collider != null)
				// 		{
				// 			if (hit.rigidbody.velocity.x > 0)
				// 				velocityEffectors_Vector2Dict["Push"].effect.x += hit.rigidbody.velocity.x + hit.rigidbody.mass + hit.rigidbody.drag * Time.deltaTime;
				// 			else
				// 				velocityEffectors_Vector2Dict["Push"].effect.x = Mathf.Clamp(velocityEffectors_Vector2Dict["Push"].effect.x + hit.rigidbody.mass + hit.rigidbody.drag, -Mathf.Infinity, Mathf.Infinity);
				// 		}
				// 		else
				// 			velocityEffectors_Vector2Dict["Push"].effect.x = 0;
				// 	}
				// 	if (rigid.velocity.y > 0)
				// 	{
				// 		topLeftColliderCorner = new Vector2(ColliderRect.xMin, ColliderRect.yMax);
				// 		topRightColliderCorner = ColliderRect.max;
				// 		hit = Physics2D.Linecast(topLeftColliderCorner, topRightColliderCorner, whatIPush);
				// 		if (hit.collider != null)
				// 		{
				// 			if (hit.rigidbody.velocity.y < 0)
				// 				velocityEffectors_Vector2Dict["Push"].effect.y += hit.rigidbody.velocity.y - hit.rigidbody.mass - hit.rigidbody.drag * Time.deltaTime;
				// 			else
				// 				velocityEffectors_Vector2Dict["Push"].effect.y = Mathf.Clamp(velocityEffectors_Vector2Dict["Push"].effect.y - hit.rigidbody.mass - hit.rigidbody.drag * Time.deltaTime, 0, Mathf.Infinity);
				// 		}
				// 		else
				// 			velocityEffectors_Vector2Dict["Push"].effect.y = 0;
				// 	}
				// 	else if (rigid.velocity.y < 0)
				// 	{
				// 		bottomLeftColliderCorner = ColliderRect.min;
				// 		bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
				// 		hit = Physics2D.Linecast(bottomLeftColliderCorner, bottomRightColliderCorner, whatIPush);
				// 		if (hit.collider != null)
				// 		{
				// 			if (hit.rigidbody.velocity.y > 0)
				// 				velocityEffectors_Vector2Dict["Push"].effect.y += hit.rigidbody.velocity.y + hit.rigidbody.mass + hit.rigidbody.drag * Time.deltaTime;
				// 			else
				// 				velocityEffectors_Vector2Dict["Push"].effect.y = Mathf.Clamp(velocityEffectors_Vector2Dict["Push"].effect.y + hit.rigidbody.mass + hit.rigidbody.drag * Time.deltaTime, 0, Mathf.Infinity);
				// 		}
				// 		else
				// 			velocityEffectors_Vector2Dict["Push"].effect.y = 0;
				// 	}
				// 	Debug.Log(velocityEffectors_Vector2Dict["Push"].effect);
				// 	break;
				case "Falling":
					// if (previousIsGrounded && !isGrounded && velocityEffectors_Vector2Dict["Jump"].effect.y == 0 && velocityEffectors_Vector2Dict["Pull Arrow"].effect.magnitude == 0 && velocityEffectors_Vector2Dict["Wind Explosion"].effect.magnitude == 0 && velocityEffectors_Vector2Dict["Wind Generator"].effect.magnitude == 0)
					// {
					// 	if (rigid.velocity.x < 0)
					// 	{
					// 		bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
					// 		hit = Physics2D.Raycast(bottomRightColliderCorner, Vector2.down, HILL_CHECK_DISTANCE, whatICollideWith);
					// 		if (hit.collider != null)
					// 		{
					// 			offsetHillCheck = hit.normal.Rotate(90);
					// 			if (offsetHillCheck.y < 0)
					// 				offsetHillCheck *= -1;
					// 			hit2 = Physics2D.Raycast(bottomLeftColliderCorner + (offsetHillCheck * Mathf.Abs(rigid.velocity.x) * Time.deltaTime), Vector2.down, HILL_CHECK_DISTANCE, whatICollideWith);
					// 			if (hit2.collider != null && Mathf.Approximately(hit.normal.y, hit2.normal.y))
					// 			{
					// 				rigid.position += (Vector3) (hit.point - bottomRightColliderCorner);
					// 				isGrounded = true;
					// 			}
					// 		}
					// 	}
					// 	else if (rigid.velocity.x > 0)
					// 	{
					// 		bottomLeftColliderCorner = ColliderRect.min;
					// 		hit = Physics2D.Raycast(bottomLeftColliderCorner, Vector2.down, HILL_CHECK_DISTANCE, whatICollideWith);
					// 		if (hit.collider != null)
					// 		{
					// 			offsetHillCheck = hit.normal.Rotate(90);
					// 			if (offsetHillCheck.y < 0)
					// 				offsetHillCheck *= -1;
					// 			hit2 = Physics2D.Raycast(bottomRightColliderCorner + (offsetHillCheck * Mathf.Abs(rigid.velocity.x) * Time.deltaTime), Vector2.down, HILL_CHECK_DISTANCE, whatICollideWith);
					// 			if (hit2.collider != null && Mathf.Approximately(hit.normal.y, hit2.normal.y))
					// 			{
					// 				rigid.position += (Vector3) (hit.point - bottomLeftColliderCorner);
					// 				isGrounded = true;
					// 			}
					// 		}
					// 	}
					// }
					velocityEffector.effect.x = 0;
					if (isSwimming || isGrounded || rigid.velocity.y > 0)
						velocityEffector.effect.y = 0;
					else if (rigid.velocity.y <= 0 && velocityEffectors_Vector2Dict["Jump"].effect.y == 0)
						velocityEffector.effect += Physics2D.gravity * Time.deltaTime * gravityScale;
					break;
				case "Jump":
					if (isSwimming)
					{
						velocityEffector.effect.y = 0;
						return;
					}
					if (velocityEffector.effect.y > -Physics2D.gravity.y * Time.deltaTime * gravityScale)
						velocityEffector.effect += Physics2D.gravity * Time.deltaTime * gravityScale;
					else
					{
						if (!isGrounded && velocityEffectors_Vector2Dict["Falling"].effect.y == 0)
							velocityEffectors_Vector2Dict["Falling"].effect.y = Physics2D.gravity.y * Time.deltaTime * gravityScale + velocityEffector.effect.y;
						velocityEffector.effect.y = 0;
					}
					break;
				case "Pull Arrow":
					if (velocityEffector.effect.sqrMagnitude == 0)
						break;
					foreach (KeyValuePair<string, VelocityEffector_Vector2> keyValuePair in velocityEffectors_Vector2Dict)
						velocityEffectors_Vector2Dict[keyValuePair.Key].effect = keyValuePair.Value.effect.ProjectWithNoNegativeScaling(velocityEffector.effect);
					break;
				default:
					if (!isSwimming)
					{
						if (velocityEffector.effect.y > -Physics2D.gravity.y * Time.deltaTime * gravityScale)
							velocityEffector.effect += Physics2D.gravity * Time.deltaTime * gravityScale;
						else if (velocityEffector.effect.y > 0)
							velocityEffector.effect.y = 0;
					}
					float slowDownRate = velocityEffectors_floatDict["Slow Down Rate"].effect;
					if (velocityEffector.effect.magnitude > slowDownRate * Time.deltaTime)
					{
						Vector2 movementVector = velocityEffectors_Vector2Dict["Movement"].effect;
						float moveSpeed = velocityEffectors_floatDict["Move Speed"].effect;
						if (velocityEffector.effect.y != 0 && movementVector.y != 0 && Mathf.Sign(movementVector.y) != Mathf.Sign(velocityEffector.effect.y))
						{
							if (Mathf.Abs(velocityEffector.effect.y) > Mathf.Abs(movementVector.y * moveSpeed * (1f / (rigid.drag + 1)) * Time.deltaTime))
								velocityEffector.effect.y += movementVector.y * Time.deltaTime;
							else
								velocityEffector.effect.y = 0;
						}
						if (velocityEffector.effect.x != 0 && movementVector.x != 0 && Mathf.Sign(movementVector.x) != Mathf.Sign(velocityEffector.effect.x))
						{
							if (Mathf.Abs(velocityEffector.effect.x) > Mathf.Abs(movementVector.x * moveSpeed * (1f / (rigid.drag + 1)) * Time.deltaTime))
								velocityEffector.effect.x += movementVector.x * Time.deltaTime;
							else
								velocityEffector.effect.x = 0;
						}
						if (isHittingWall)
							velocityEffector.effect.x = 0;
						if (velocityEffector.effect.magnitude > slowDownRate * Time.deltaTime)
							velocityEffector.effect = velocityEffector.effect.normalized * (velocityEffector.effect.magnitude - (slowDownRate * Time.deltaTime));
						else
							velocityEffector.effect = Vector2.zero;
					}
					else
						velocityEffector.effect = Vector2.zero;
					break;
			}
		}

		public virtual void HandleJumping ()
		{
		}

		public virtual void StartJump ()
		{
			if (!canMoveAndJump)
				return;
			if (animator != null)
				animator.SetTrigger(jumpAnimTriggerName);
			velocityEffectors_Vector2Dict["Jump"].effect.y = jumpSpeed;
			isJumping = true;
		}

		public virtual void StopJump ()
		{
			if (!canMoveAndJump)
				return;
			if (animator != null)
				animator.ResetTrigger(jumpAnimTriggerName);
			velocityEffectors_Vector2Dict["Jump"].effect.y = 0;
			isJumping = false;
		}

		public virtual void SetIsGrounded ()
		{
			bottomLeftColliderCorner = ColliderRect.min;
			bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
			Vector2 offsetPhysicsQuery = Vector2.down * groundLinecastOffset.positionChangeAlongPerpendicular;
			contactFilter = new ContactFilter2D();
			contactFilter.layerMask = whatICollideWith;
			contactFilter.useLayerMask = true;
			isGrounded = Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0;
			if (isGrounded)
				rigid.gravityScale = 1;
			else
				rigid.gravityScale = 0;
			// Debug.DrawLine(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), Color.red, 0);
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			for (int i = 0; i < coll.contactCount; i ++)
			{
				float hitNormalY = coll.GetContact(i).normal.y;
				if (hitNormalY >= Vector2.one.normalized.y)
				{
					foreach (KeyValuePair<string, VelocityEffector_Vector2> keyValuePair in velocityEffectors_Vector2Dict)
						velocityEffectors_Vector2Dict[keyValuePair.Key].effect.y = Mathf.Max(0, velocityEffectors_Vector2Dict[keyValuePair.Key].effect.y);
					return;
				}
				else if (hitNormalY <= -Vector2.one.normalized.y)
				{
					foreach (KeyValuePair<string, VelocityEffector_Vector2> keyValuePair in velocityEffectors_Vector2Dict)
						velocityEffectors_Vector2Dict[keyValuePair.Key].effect.y = Mathf.Min(0, velocityEffectors_Vector2Dict[keyValuePair.Key].effect.y);
					return;
				}
			}
		}

#if UNITY_EDITOR
		void OnValidate ()
		{
			if (!Selection.gameObjects.Contains(gameObject))
				return;
			Vector2 offsetPhysicsQuery = Vector2.down * groundLinecastOffset.positionChangeAlongPerpendicular;
			bottomLeftColliderCorner = ColliderRect.min;
			bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
			Debug.DrawLine(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.right * groundLinecastOffset.lengthChange2), Color.red, 1);
			offsetPhysicsQuery = Vector2.up * roofLinecastOffset.positionChangeAlongPerpendicular;
			topLeftColliderCorner = new Vector2(ColliderRect.xMin, ColliderRect.yMax);
			topRightColliderCorner = ColliderRect.max;
			Debug.DrawLine(topLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * roofLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.right * roofLinecastOffset.lengthChange2), Color.red, 1);
			offsetPhysicsQuery = Vector2.right * wallLinecastOffset.positionChangeAlongPerpendicular;
			Debug.DrawLine(bottomRightColliderCorner + offsetPhysicsQuery + (Vector2.down * wallLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.up * wallLinecastOffset.lengthChange2), Color.red, 1);
			offsetPhysicsQuery = Vector2.left * wallLinecastOffset.positionChangeAlongPerpendicular;
			Debug.DrawLine(bottomLeftColliderCorner + offsetPhysicsQuery + (Vector2.down * wallLinecastOffset.lengthChange1), topLeftColliderCorner + offsetPhysicsQuery + (Vector2.up * wallLinecastOffset.lengthChange2), Color.red, 1);
		}
#endif

		[Serializable]
		public class VelocityEffector<T>
		{
			public string name;
			public T effect;

			public VelocityEffector (string name, T effect)
			{
				this.name = name;
				this.effect = effect;
			}
		}

		[Serializable]
		public class VelocityEffector_Vector2 : VelocityEffector<Vector2>
		{
			public VelocityEffector_Vector2 (string name, Vector2 effect) : base (name, effect)
			{
			}
		}

		[Serializable]
		public class VelocityEffector_float : VelocityEffector<float>
		{
			public VelocityEffector_float (string name, float effect) : base (name, effect)
			{
			}
		}

		[Serializable]
		public class LinecastOffset
		{
			public float positionChangeAlongPerpendicular;
			public float lengthChange1;
			public float lengthChange2;
		}
	}
}