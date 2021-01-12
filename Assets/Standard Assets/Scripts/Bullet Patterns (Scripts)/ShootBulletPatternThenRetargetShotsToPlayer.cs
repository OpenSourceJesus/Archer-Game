using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootBulletPatternThenRetargetShotsToPlayer : BulletPattern
	{
		public BulletPattern bulletPattern;
		[MakeConfigurable]
		public float retargetTime;
		public bool lastRetarget;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			Bullet[] output = bulletPattern.Shoot (spawner, bulletPrefab);
			foreach (Bullet bullet in output)
				bullet.StartCoroutine(RetargetAfterDelay (bullet, retargetTime, lastRetarget));
			return output;
		}
		
		public override Vector2 GetRetargetDirection (Bullet bullet)
		{
			return Player.instance.trs.position - bullet.trs.position;
		}
	}
}