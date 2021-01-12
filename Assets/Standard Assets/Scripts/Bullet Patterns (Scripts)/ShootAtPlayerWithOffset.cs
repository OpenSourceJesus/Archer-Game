using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootAtPlayerWithOffset : ShootAtPlayer
	{
		[MakeConfigurable]
		public float shootOffset;
		
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return base.GetShootDirection(spawner).Rotate(shootOffset);
		}
	}
}