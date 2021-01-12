using System;
using UnityEngine;

namespace ArcherGame
{
	[RequireComponent(typeof(Spawnable))]
	public class SpawnableHazard : Hazard//, ISpawnable
	{
		public Spawnable spawnable;
//		public int prefabIndex;
//		public int PrefabIndex
//		{
//			get
//			{
//				return prefabIndex;
//			}
//		}
		public TemporaryActiveObject temporaryActiveObject;

		public virtual void Start ()
		{
			if (temporaryActiveObject != null)
				temporaryActiveObject.Do ();
		}

		public virtual void OnDisable ()
		{
			Destroy(gameObject);
		}
	}
}