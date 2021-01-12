using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
    //[ExecuteInEditMode]
	public class WindArrow : Arrow
	{
		public WindExplosion windExplosionPrefab;

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			base.OnCollisionEnter2D (coll);
			Destroy(gameObject);
		}

		public override void OnDisable ()
		{
			base.OnDisable ();
			WindExplosion windExplosion = ObjectPool.instance.SpawnComponent<WindExplosion>(windExplosionPrefab.prefabIndex, trs.position, Quaternion.identity);
			windExplosion.SetForce (Mathf.Sqrt(speedSqr));
			windExplosion.collider.enabled = true;
		}
	}
}