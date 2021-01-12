using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootAtPlayerWithOffsetThenDespawnAndSplitInArcAimedTowardsPlayerWithOffset : ShootAtPlayerWithOffset
	{
		[MakeConfigurable]
		public float splitOffset;
		public Bullet splitBulletPrefab;
		[MakeConfigurable]
		public float splitDelay;
		[MakeConfigurable]
		public float splitArc;
		[MakeConfigurable]
		public float splitNumber;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			Bullet[] output = base.Shoot (spawner, bulletPrefab);
			foreach (Bullet bullet in output)
				GameManager.Instance.StartCoroutine(SplitAfterDelay (bullet, splitBulletPrefab, splitDelay));
			return output;
		}
		
		public override Bullet[] Split (Bullet bullet, Vector2 direction, Bullet splitBulletPrefab)
		{
			Bullet[] output = new Bullet[0];
			float toPlayer = (Player.instance.trs.position - bullet.trs.position).GetFacingAngle();
			for (float splitAngle = toPlayer - splitArc / 2 + splitOffset; splitAngle < toPlayer + splitArc / 2 + splitOffset; splitAngle += splitArc / splitNumber)
				output = base.Split (bullet, VectorExtensions.FromFacingAngle(splitAngle), splitBulletPrefab);
			ObjectPool.instance.Despawn (bullet.prefabIndex, bullet.gameObject, bullet.trs);
			return output;
		}
	}
}