using UnityEngine;
using System;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class SporeTrap : ShooterTrap
	{
		public Transform explodeVisualizerTrs;

		public override void Start ()
		{
			base.Start ();
	#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Bomb bomb = bulletPrefab as Bomb;
				explodeVisualizerTrs.transform.localScale = Vector2.one * bomb.explodeSize;
				return;
			}
	#endif
		}
	}
}