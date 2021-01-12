using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class GroundEnemy : AwakableEnemy
	{
		public override void HandleJumping ()
		{
			if (isDead)
				return;
			SetIsHittingWall ();
			if (Player.instance.ColliderRect.min.y > ColliderRect.max.y || isHittingWall)
			{
				if (!isJumping && isGrounded)
					StartJump ();
			}
			else if (isJumping)
				StopJump ();
			if (rigid.velocity.y <= 0)
				isJumping = false;
		}

		public override void Awaken ()
		{
			base.Awaken ();
			velocityEffectors_floatDict["Gravity Scale"].effect = 1;
		}
	}
}