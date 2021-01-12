using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootAtPlayer : BulletPattern
	{
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return Player.instance.trs.position - spawner.position;
		}
	}
}