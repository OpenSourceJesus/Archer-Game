using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootAtPlayerWithRandomOffset : ShootAtPlayer
	{
		[MakeConfigurable]
		public float randomShootOffsetRange;
		
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return VectorExtensions.Rotate(base.GetShootDirection(spawner), Random.Range(-randomShootOffsetRange / 2, randomShootOffsetRange / 2));
		}
	}
}