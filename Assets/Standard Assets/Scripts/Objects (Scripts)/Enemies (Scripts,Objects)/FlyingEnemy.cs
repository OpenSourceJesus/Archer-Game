using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class FlyingEnemy : AwakableEnemy
	{
		public float rotateRate;
		public Transform facingDirectionTrs;

		public override void Init ()
		{
			base.Init ();
			initRotation = facingDirectionTrs.eulerAngles.z;
		}

		public override void DoUpdate ()
		{
			if (!awake || GameManager.paused || isDead)// || this == null)
				return;
			base.DoUpdate ();
			HandleRotation ();
		}

		public override void Reset ()
		{
			base.Reset ();
			facingDirectionTrs.eulerAngles = Vector3.forward * initRotation;
		}

		public virtual void HandleRotation ()
		{
			if (velocityEffectors_Vector2Dict["Pull Arrow"].effect.magnitude == 0)
				facingDirectionTrs.up = facingDirectionTrs.up.RotateTo(Player.instance.trs.position - trs.position, rotateRate * Time.deltaTime);
		}

		public override void HandleVelocity ()
		{
			if (facingDirectionTrs == null)
				return;
			Move (facingDirectionTrs.up);
			SetIsGrounded ();
			SetIsHittingWall ();
			if (velocityEffectors_Vector2Dict["Pull Arrow"].effect.magnitude == 0)
			{
				SetVelocityEffector (velocityEffectors_Vector2Dict["Wind Explosion"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Water"]);
				rigid.velocity = velocityEffectors_Vector2Dict["Movement"].effect + velocityEffectors_Vector2Dict["Wind Explosion"].effect + velocityEffectors_Vector2Dict["Water"].effect;
			}
			else
			{
				velocityEffectors_Vector2Dict["Falling"].effect = Vector2.zero;
				velocityEffectors_Vector2Dict["Wind Explosion"].effect = Vector2.zero;
				velocityEffectors_Vector2Dict["Water"].effect = Vector2.zero;
				velocityEffectors_Vector2Dict["Movement"].effect = Vector2.zero;
				rigid.velocity = velocityEffectors_Vector2Dict["Pull Arrow"].effect;
			}
		}

		public override void SetVelocityEffector (VelocityEffector_Vector2 velocityEffector)
		{
			if (velocityEffector.effect.y > 0)
			{
				Vector2 offsetPhysicsQuery = Vector2.up * roofLinecastOffset.positionChangeAlongPerpendicular;
				topLeftColliderCorner = new Vector2(ColliderRect.xMin, ColliderRect.yMax);
				topRightColliderCorner = ColliderRect.max;
				contactFilter = new ContactFilter2D();
				contactFilter.layerMask = whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(topLeftColliderCorner + offsetPhysicsQuery + (Vector2.left * roofLinecastOffset.lengthChange1), topRightColliderCorner + offsetPhysicsQuery + (Vector2.right * roofLinecastOffset.lengthChange1), contactFilter, new RaycastHit2D[1]) > 0)
					velocityEffector.effect.y = 0;
			}
			else if (velocityEffector.effect.y < 0)
			{
				Vector2 offsetPhysicsQueries = Vector2.down * groundLinecastOffset.positionChangeAlongPerpendicular;
				bottomLeftColliderCorner = ColliderRect.min;
				bottomRightColliderCorner = new Vector2(ColliderRect.xMax, ColliderRect.yMin);
				contactFilter = new ContactFilter2D();
				contactFilter.layerMask = whatICollideWith;
				contactFilter.useLayerMask = true;
				if (Physics2D.Linecast(bottomLeftColliderCorner + offsetPhysicsQueries + (Vector2.left * groundLinecastOffset.lengthChange1), bottomRightColliderCorner + offsetPhysicsQueries + (Vector2.up * groundLinecastOffset.lengthChange2), contactFilter, new RaycastHit2D[1]) > 0)
					velocityEffector.effect.y = 0;
			}
			switch (velocityEffector.name)
			{
				default:
					break;
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
				// 				trs.position += (Vector3) (hit.point - bottomRightColliderCorner);
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
				// 				trs.position += (Vector3) (hit.point - bottomLeftColliderCorner);
				// 				isGrounded = true;
				// 			}
				// 		}
				// 	}
				// }
				if (isSwimming || isGrounded || rigid.velocity.y > 0)
					velocityEffector.effect.y = 0;
				else if (rigid.velocity.y <= 0 && velocityEffectors_Vector2Dict["Jump"].effect.y == 0)
					velocityEffector.effect += Physics2D.gravity * Time.deltaTime * velocityEffectors_floatDict["Gravity Scale"].effect;
					break;
				case "Wind Explosion":
				case "Water":
					if (velocityEffector.effect.magnitude > velocityEffectors_floatDict["Slow Down Rate"].effect * Time.deltaTime)
					{
						if (velocityEffector.effect.y != 0 && velocityEffectors_Vector2Dict["Movement"].effect.y != 0 && Mathf.Sign(velocityEffectors_Vector2Dict["Movement"].effect.y) != Mathf.Sign(velocityEffector.effect.y))
						{
							if (Mathf.Abs(velocityEffector.effect.y) > Mathf.Abs(velocityEffectors_Vector2Dict["Movement"].effect.y * velocityEffectors_floatDict["Move Speed"].effect * (1f / (rigid.drag + 1)) * Time.deltaTime))
								velocityEffector.effect.y += velocityEffectors_Vector2Dict["Movement"].effect.y * Time.deltaTime;
							else
								velocityEffector.effect.y = 0;
						}
						if (velocityEffector.effect.x != 0 && velocityEffectors_Vector2Dict["Movement"].effect.x != 0 && Mathf.Sign(velocityEffectors_Vector2Dict["Movement"].effect.x) != Mathf.Sign(velocityEffector.effect.x))
						{
							if (Mathf.Abs(velocityEffector.effect.x) > Mathf.Abs(velocityEffectors_Vector2Dict["Movement"].effect.x * velocityEffectors_floatDict["Move Speed"].effect * (1f / (rigid.drag + 1)) * Time.deltaTime))
								velocityEffector.effect.x += velocityEffectors_Vector2Dict["Movement"].effect.x * Time.deltaTime;
							else
								velocityEffector.effect.x = 0;
						}
						// if (Mathf.Abs(rigid.velocity.y) > Mathf.Abs(velocityEffector.previousEffect.y))
						// {
						// 	rigid.velocity -= Vector2.up * velocityEffector.previousEffect.y;
						// 	if (Mathf.Abs(velocityEffector.effect.y) > Mathf.Abs(velocityEffector.previousEffect.y))
						// 		velocityEffector.effect.y -= velocityEffector.previousEffect.y;
						// 	else
						// 		velocityEffector.effect.y = 0;
						// }
						// else
						// 	rigid.velocity = rigid.velocity.SetY(0);
						if (isHittingWall)
							velocityEffector.effect.x = 0;
						if (velocityEffector.effect.magnitude > velocityEffectors_floatDict["Slow Down Rate"].effect * Time.deltaTime)
							velocityEffector.effect = velocityEffector.effect.normalized * (velocityEffector.effect.magnitude - (velocityEffectors_floatDict["Slow Down Rate"].effect * Time.deltaTime));
						else
							velocityEffector.effect = Vector2.zero;
					}
					else
						velocityEffector.effect = Vector2.zero;
					break;
			}
		}

		public override void Move (Vector2 move)
		{
			move = Vector2.ClampMagnitude(move, 1);
			bool shouldPlayMovementAnim = true;
			VelocityEffector_float moveSpeedVelocityEffector;
			if (!velocityEffectors_floatDict.TryGetValue("Move Speed", out moveSpeedVelocityEffector))
				return;
			if (((Vector2) (Player.instance.trs.position - trs.position)).sqrMagnitude > minChaseDistFromPlayerSqr && ((Vector2) Player.instance.trs.position - ((Vector2) trs.position + (move * moveSpeedVelocityEffector.effect * Time.deltaTime))).sqrMagnitude > minChaseDistFromPlayerSqr)
				velocityEffectors_Vector2Dict["Movement"].effect = move * moveSpeedVelocityEffector.effect;
			else if (((Vector2) (Player.instance.trs.position - trs.position)).sqrMagnitude < maxEsacpeDistFromPlayerSqr && ((Vector2) Player.instance.trs.position - ((Vector2) trs.position - (move * moveSpeedVelocityEffector.effect * Time.deltaTime))).sqrMagnitude < maxEsacpeDistFromPlayerSqr)
				velocityEffectors_Vector2Dict["Movement"].effect = -move * moveSpeedVelocityEffector.effect;
			else
			{
				shouldPlayMovementAnim = false;
				velocityEffectors_Vector2Dict["Movement"].effect = Vector2.zero;
			}
			if (animator != null)
			{
				if (shouldPlayMovementAnim)
				{
					animator.ResetTrigger("toEXIT");
					animator.SetTrigger("toFLYING");
					animator.SetFloat("moveSpeed", move.magnitude);
				}
				else
				{
					animator.ResetTrigger("toFLYING");
					animator.SetTrigger("toEXIT");
					animator.SetFloat("moveSpeed", 0);
				}
			}
		}
	}
}