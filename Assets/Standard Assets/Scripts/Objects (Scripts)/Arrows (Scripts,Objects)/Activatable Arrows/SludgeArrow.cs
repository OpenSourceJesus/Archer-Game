using System.Collections;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class SludgeArrow : ActivatableArrow
	{
		protected bool inSludge;
		public float linearDragIncreaseRate;
		public float angularDragIncreaseRate;
		public float growRate;
		public float rotateRate;

		public override void Activate ()
		{
			if (inSludge)
			{
				rotateRate *= -1;
				return;
			}
			inSludge = true;
			StartCoroutine(SludgeRoutine ());
			StartCoroutine(MakeCollidable ());
		}

		public virtual IEnumerator MakeCollidable ()
		{
			gameObject.layer = LayerMask.NameToLayer("Sludge Arrow");
			ContactFilter2D contactFilter = new ContactFilter2D();
			contactFilter.layerMask = LayerMask.GetMask("Player");
			contactFilter.useLayerMask = true;
			Physics2D.IgnoreCollision(Player.instance.collider, collider, true);
			yield return new WaitUntil(() => (collider.OverlapCollider(contactFilter, new Collider2D[1]) == 0));
			Physics2D.IgnoreCollision(Player.instance.collider, collider, false);
		}

		public virtual IEnumerator SludgeRoutine ()
		{
			do
			{
				rigid.drag += linearDragIncreaseRate * Time.deltaTime;
				rigid.angularDrag += angularDragIncreaseRate * Time.deltaTime;
				if (trs.parent == null)
					trs.localScale += (Vector3) Vector2.one * growRate * Time.deltaTime;
				else
					trs.SetWorldScale (trs.lossyScale + (Vector3) (Vector2.one * growRate * Time.deltaTime));
				trs.eulerAngles += Vector3.forward * rotateRate * Time.deltaTime;
				yield return new WaitForEndOfFrame();
			} while (true);
		}

        public override void Deactivate ()
        {
            Activate ();
        }

		public override void OnDisable ()
		{
			base.OnDisable ();
			StopCoroutine(SludgeRoutine ());
			StopCoroutine(MakeCollidable ());
			gameObject.layer = LayerMask.NameToLayer("Arrow");
			rigid.drag = 0;
			rigid.angularDrag = 0;
			trs.localScale = Vector3.one * trs.localScale.z;
			inSludge = false;
		}
	}
}