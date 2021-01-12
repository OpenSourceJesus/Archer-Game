using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class DeadlyTouchFlyingEnemy : FlyingEnemy
	{
		public Transform damageDirectionIndicatorTrs;
		public float offsetDamageDirectionTrsRotation;

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			base.OnCollisionEnter2D (coll);
			OnCollisionStay2D (coll);
		}

		public virtual void OnCollisionStay2D (Collision2D coll)
		{
			Player player = coll.gameObject.GetComponent<Player>();
			if (player != null)
			{
				damageDirectionIndicatorTrs.up = (player.trs.position - damageDirectionIndicatorTrs.position).Rotate(offsetDamageDirectionTrsRotation);
				animator.ResetTrigger(idleAnimTriggerName);
				animator.SetTrigger(attackAnimTriggerName);
				damageDirectionIndicatorTrs.gameObject.SetActive(true);
				player.TakeDamage ();
			}
		}

		public virtual void OnCollisionExit2D (Collision2D coll)
		{
			Player player = coll.gameObject.GetComponent<Player>();
			if (player != null)
			{
				animator.ResetTrigger(attackAnimTriggerName);
				animator.SetTrigger(idleAnimTriggerName);
				damageDirectionIndicatorTrs.gameObject.SetActive(false);
			}
		}

		public override void Death ()
		{
			base.Death ();
			damageDirectionIndicatorTrs.gameObject.SetActive(false);
		}
	}
}