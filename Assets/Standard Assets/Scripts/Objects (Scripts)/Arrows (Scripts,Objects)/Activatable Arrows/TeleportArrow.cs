using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using DialogAndStory;

namespace ArcherGame
{
	public class TeleportArrow : ActivatableArrow
	{
		Collider2D[] hits = new Collider2D[1];
		ContactFilter2D contactFilter = new ContactFilter2D();
		public static bool justTeleported;

		public override void Activate ()
		{
			contactFilter.useTriggers = false;
			contactFilter.useLayerMask = true;
			contactFilter.layerMask = Player.instance.whatICollideWith;
			if (Physics2D.OverlapBox(trs.position, Player.instance.UnrotatedColliderRect.size, 0, contactFilter, hits) > 0)
			{
				Destroy(gameObject);
				Player.instance.HandleShooting ();
			}
			else
			{
				if (ArrowCollectible.tutorialConversationsDict[GetType()].lastStartedDialog != null)
					DialogManager.Instance.EndConversation (ArrowCollectible.tutorialConversationsDict[GetType()]);
				Player.instance.trs.position = trs.position;
				justTeleported = true;
				Player.instance.CurrentArrowEntry.canShoot = false;
				Destroy(gameObject);
			}
		}

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			if (coll.gameObject.GetComponent<Enemy>() != null)
			{
				rigid.bodyType = RigidbodyType2D.Kinematic;
				collider.isTrigger = true;
				trs.SetParent(coll.transform);
			}
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}