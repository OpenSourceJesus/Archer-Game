using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class AimAtPlayer : ShootAtPlayer
	{
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			spawner.up = GetShootDirection(spawner);
			return new Bullet[0];
		}
	}
}