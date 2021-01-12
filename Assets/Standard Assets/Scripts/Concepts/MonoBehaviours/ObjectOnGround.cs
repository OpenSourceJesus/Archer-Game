using UnityEngine;
using Extensions;

public class ObjectOnGround : MonoBehaviour
{
	public Transform trs;
	public new Collider2D collider;

	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (trs == null)
			trs = GetComponent<Transform>();
		if (collider == null)
			collider = GetComponent<Collider2D>();
#endif
		RaycastHit2D hit = Physics2D.Raycast(trs.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Wall"));
		if (hit.collider != null)
		{
			trs.position = collider.GetRect().AnchorToPoint(hit.point, new Vector2(.5f, 0)).center;
			Destroy(this);
		}
	}
}