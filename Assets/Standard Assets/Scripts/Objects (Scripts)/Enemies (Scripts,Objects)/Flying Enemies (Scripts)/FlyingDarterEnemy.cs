using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class FlyingDarterEnemy : FlyingEnemy
	{
		public float dartStopDelay;
		bool isDarting;
		public float maxDartDist;

		public override void DoUpdate ()
		{
			base.DoUpdate ();
			HandleDarting ();
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		Collider2D hit;
		public virtual void HandleDarting ()
		{
			if (this == null)
				return;
			hit = Physics2D.OverlapBox(trs.position + (facingDirectionTrs.up * maxDartDist / 2), UnrotatedColliderRect.size, facingDirectionTrs.eulerAngles.z, LayerMask.GetMask("Player"));
			if (hit != null)
			{
				StopCoroutine(DartRoutine ());
				StartCoroutine(DartRoutine ());
			}
		}

		public override void HandleRotation ()
		{
			if (!isDarting)
				base.HandleRotation ();
		}

		public virtual IEnumerator DartRoutine ()
		{
			isDarting = true;
			bool previousShouldStop = false;
			float timeSinceShouldStop = Mathf.Infinity;
			do
			{
				Move (trs.up);
				hit = Physics2D.OverlapBox(trs.position + (facingDirectionTrs.up * maxDartDist / 2), UnrotatedColliderRect.size, facingDirectionTrs.eulerAngles.z, LayerMask.GetMask("Player"));
				yield return new WaitForEndOfFrame();
				if (hit == null)
				{
					if (!previousShouldStop)
						timeSinceShouldStop = Time.time;
					else if (Time.time - timeSinceShouldStop > dartStopDelay)
						break;
				}
				previousShouldStop = hit == null;
			} while (true);
			isDarting = false;
		}

		public override void HandleVelocity ()
		{
			Move (trs.up);
			SetIsGrounded ();
			SetIsHittingWall ();
			if (velocityEffectors_Vector2Dict["Pull Arrow"].effect.magnitude == 0)
			{
				SetVelocityEffector (velocityEffectors_Vector2Dict["Wind Explosion"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Wind Generator"]);
				SetVelocityEffector (velocityEffectors_Vector2Dict["Water"]);
				rigid.velocity = velocityEffectors_Vector2Dict["Movement"].effect + velocityEffectors_Vector2Dict["Wind Explosion"].effect + velocityEffectors_Vector2Dict["Wind Generator"].effect + velocityEffectors_Vector2Dict["Water"].effect;
			}
			else
			{
				velocityEffectors_Vector2Dict["Wind Explosion"].effect = Vector2.zero;
				velocityEffectors_Vector2Dict["Wind Generator"].effect = Vector2.zero;
				velocityEffectors_Vector2Dict["Water"].effect = Vector2.zero;
				velocityEffectors_Vector2Dict["Movement"].effect = Vector2.zero;
				rigid.velocity = velocityEffectors_Vector2Dict["Pull Arrow"].effect;
			}
		}

		public override void Move (Vector2 move)
		{
			move = Vector2.ClampMagnitude(move, 1);
			if (isDarting)
				velocityEffectors_Vector2Dict["Movement"].effect = move * velocityEffectors_floatDict["Dart Speed"].effect;
			else
				velocityEffectors_Vector2Dict["Movement"].effect = move * velocityEffectors_floatDict["Move Speed"].effect;
		}
	}
}