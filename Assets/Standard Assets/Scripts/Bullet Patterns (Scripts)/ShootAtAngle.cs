using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootAtAngle : BulletPattern
	{
        public float angle;

		public override Vector2 GetShootDirection (Transform spawner)
		{
			return VectorExtensions.FromFacingAngle(angle);
		}
	}
}