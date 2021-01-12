using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootWhereFacingWithRandomOffset : ShootWhereFacing
	{
		public float shootOffsetRange;
		
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return VectorExtensions.Rotate(base.GetShootDirection(spawner), Random.Range(-shootOffsetRange / 2, shootOffsetRange / 2));
		}
	}
}