using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class LaserMagicSpire : MonoBehaviour
	{
		public Transform trs;
		public Timer shootTimer;
		public Laser laserPrefab;
		bool playerIsInRange;
		public float laserDuration;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			shootTimer.onFinished += Shoot;
		}

		public virtual void Shoot (params object[] args)
		{
			if (playerIsInRange)
			{
				Laser laser = ObjectPool.instance.SpawnComponent<Laser>(laserPrefab.prefabIndex, trs.position, Quaternion.LookRotation(Vector3.forward, Player.instance.trs.position - trs.position));
				laser.duration = laserDuration;
				Destroy(laser.gameObject, laserDuration);
				shootTimer.Reset ();
				shootTimer.Start ();
			}
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			playerIsInRange = true;
			shootTimer.Start ();
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			playerIsInRange = false;
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			shootTimer.onFinished -= Shoot;
		}
	}
}