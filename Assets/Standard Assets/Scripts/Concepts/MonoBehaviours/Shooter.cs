using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Shooter : MonoBehaviour
	{
        public Transform trs;
		public Bullet bulletPrefab;
		public Timer shootTimer;
		public BulletPattern bulletPattern;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
            {
                if (bulletPattern != null)
                    bulletPattern.Init (trs);
            	return;
            }
#endif
			shootTimer.onFinished += Shoot;
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			shootTimer.onFinished -= Shoot;
		}

		public virtual void Shoot ()
		{
			Shoot (new object[0]);
		}

		public virtual void Shoot (params object[] args)
		{
			bulletPattern.Shoot (trs, bulletPrefab);
		}
	}
}