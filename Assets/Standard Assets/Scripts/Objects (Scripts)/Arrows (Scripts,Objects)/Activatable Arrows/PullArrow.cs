using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class PullArrow : ActivatableArrow
	{
		public float pullSpeed;
		Enemy stuckInEnemy;

		public override void Activate ()
		{
			// ContactFilter2D contactFilter = new ContactFilter2D();
			// contactFilter.layerMask = LayerMask.GetMask("Player");
			// contactFilter.useLayerMask = true;
			// if (collider.OverlapCollider(contactFilter, new Collider2D[1]) == 1)
			// 	Destroy(gameObject);
			// else
				StartCoroutine(PullRoutine ());
		}

		public virtual IEnumerator PullRoutine ()
		{
			rigid.gravityScale = 0;
			Player.instance.canMoveAndJump = false;
			do
			{
				if (collider.isTrigger)
				{
					stuckInEnemy = trs.parent.GetComponent<Enemy>();
					if (stuckInEnemy != null)
						stuckInEnemy.velocityEffectors_Vector2Dict["Pull Arrow"].effect = (Vector2) (Player.instance.trs.position - trs.position).normalized * pullSpeed;
				}
				else
				{
					rigid.drag = 0;
					rigid.angularDrag = float.MaxValue;
					rigid.velocity = (Vector2) (Player.instance.trs.position - trs.position).normalized * pullSpeed;
				}
				Player.instance.velocityEffectors_Vector2Dict["Pull Arrow"].effect = (Vector2) (trs.position - Player.instance.trs.position).normalized * pullSpeed;
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		public override void Deactivate ()
		{
			Player.instance.CurrentArrowEntry.canShoot = false;
			Destroy (gameObject);
		}

		public override void OnDisable ()
		{
			base.OnDisable ();
			rigid.gravityScale = 1;
			StopAllCoroutines();
			// StopCoroutine(PullRoutine ());
			Player.instance.canMoveAndJump = true;
			Player.instance.velocityEffectors_Vector2Dict["Pull Arrow"].effect = Vector2.zero;
			if (stuckInEnemy != null)
				stuckInEnemy.velocityEffectors_Vector2Dict["Pull Arrow"].effect = Vector2.zero;
		}

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			if (coll.gameObject.GetComponent<Enemy>() != null)
			{
				collider.isTrigger = true;
				rigid.bodyType = RigidbodyType2D.Kinematic;
				rigid.velocity = Vector2.zero;
				trs.SetParent(coll.transform);
			}
		}
	}
}