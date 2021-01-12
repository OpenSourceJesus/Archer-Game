using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;

namespace ArcherGame
{
	public class FlyingTeleportEnemy : FlyingEnemy
	{
		public float teleportRange;
		public float teleportReload;
		public AnimationState teleportAnimationState;
		public AnimationState teleportedAnimationState;
		float timeOfLastTeleport;

		public override void Awaken ()
		{
			base.Awaken ();
			StartCoroutine(HandleTeleportingRoutine ());
		}

		public virtual IEnumerator HandleTeleportingRoutine ()
		{
			do
			{
				if (ShouldTeleport())
				{
					canMoveAndJump = false;
					if (!teleportAnimationState.isNull)
					{
						animator.Play(teleportAnimationState.name);
						yield return new WaitUntil(() => (!animator.GetCurrentAnimatorStateInfo(teleportAnimationState.layerIndex).IsName(teleportAnimationState.name)));
					}
					CoroutineWithData cd = new CoroutineWithData(this, GetTeleportPositionRoutine());
					do
					{
						yield return cd.coroutine;
						if (cd.result is Vector2)
						{
							trs.position = (Vector2) cd.result;
							break;
						}
					} while (true);
					if (!teleportedAnimationState.isNull)
					{
						animator.Play(teleportedAnimationState.name);
						yield return new WaitUntil(() => (!animator.GetCurrentAnimatorStateInfo(teleportedAnimationState.layerIndex).IsName(teleportedAnimationState.name)));
					}
					timeOfLastTeleport = Time.time;
				}
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		public virtual bool ShouldTeleport ()
		{
			return Time.time  - timeOfLastTeleport > teleportReload;
		}

		public virtual IEnumerator GetTeleportPositionRoutine ()
		{
			Vector2 output;
			ContactFilter2D contactFilter = new ContactFilter2D();
			contactFilter.layerMask = LayerMask.GetMask("Player", "Wall", "Breakable Tile", "Move Tile");
			contactFilter.useLayerMask = true;
			do
			{
				output = (Vector2) trs.position + Random.insideUnitCircle.normalized * teleportRange;
				yield return new WaitForEndOfFrame();
			} while (Physics2D.OverlapBox(output, ColliderRect.size, 0, whatICollideWith) != null || Physics2DExtensions.LinecastWithWidth(output, Player.instance.trs.position, shooter.bulletPrefab.boundsSize.x, contactFilter, new List<RaycastHit2D>()) > 1);
			yield return output;
		}
	}
}