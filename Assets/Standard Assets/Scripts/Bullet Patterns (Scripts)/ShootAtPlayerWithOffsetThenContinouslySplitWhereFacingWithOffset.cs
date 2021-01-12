using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootAtPlayerWithOffsetThenContinouslySplitWhereFacingWithOffset : ShootAtPlayerWithOffset
	{
		[MakeConfigurable]
		public float splitOffset;
		public Bullet splitBulletPrefab;
		[MakeConfigurable]
		public float splitDelay;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			Bullet[] output = base.Shoot(spawner, bulletPrefab);
			foreach (Bullet bullet in output)
				bullet.StartCoroutine(SplitAfterDelay (bullet, splitBulletPrefab, splitDelay));
			return output;
		}
		
		public override IEnumerator SplitAfterDelay (Bullet bullet, Bullet splitBulletPrefab, float delay)
		{
			while (true)
			{
				yield return new WaitForSeconds(delay);
				if (!bullet.gameObject.activeSelf)
					yield break;
				yield return Split (bullet, splitBulletPrefab);
			}
		}
		
		public override Vector2 GetSplitDirection (Bullet bullet)
		{
			return bullet.trs.up.Rotate(splitOffset);
		}
	}
}