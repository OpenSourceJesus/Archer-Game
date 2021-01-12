using UnityEngine;
using System;
using System.Collections;
using Extensions;

namespace ArcherGame
{
	public class GroundSpawnerEnemy : GroundEnemy
	{
		public SpawnEntry[] spawnEntries;

		public override void Start ()
		{
			base.Start ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				for (int i = 0; i < spawnEntries.Length; i ++)
					spawnEntries[i].spawnRange = UnrotatedColliderRect.size.magnitude / 2 + spawnEntries[i].enemyPrefab.UnrotatedColliderRect.size.magnitude / 2;
				return;
			}
#endif
		}

		public override void Awaken ()
		{
			base.Awaken ();
			foreach (SpawnEntry spawnEntry in spawnEntries)
				StartCoroutine(spawnEntry.SpawnRoutine ());
		}

		public override void Reset ()
		{
			StopAllCoroutines();
			foreach (SpawnEntry spawnEntry in spawnEntries)
			{
				for (int i = 0; i < spawnEntry.enemies.Length; i ++)
					Destroy(spawnEntry.enemies[i]);
				spawnEntry.enemies = new Enemy[0];
			}
			base.Reset ();
		}

		[Serializable]
		public class SpawnEntry
		{
			public GroundSpawnerEnemy groundSpawnerEnemy; 
			public Enemy enemyPrefab;
			public float reloadDuration;
			[HideInInspector]
			public Enemy[] enemies = new Enemy[0];
			[HideInInspector]
			public float spawnRange;

			public virtual IEnumerator SpawnRoutine ()
			{
				do
				{
					yield return new WaitForSeconds(reloadDuration);
					Spawn ();
				} while (true);
			}

			public virtual void Spawn ()
			{
				Enemy enemy = ObjectPool.instance.SpawnComponent<Enemy>(enemyPrefab.prefabIndex, groundSpawnerEnemy.trs.position);
				Physics2D.IgnoreCollision(enemy.collider, groundSpawnerEnemy.collider);
				AwakableEnemy awakableEnemy = enemy as AwakableEnemy;
				if (awakableEnemy != null)
					awakableEnemy.Awaken ();
				enemies = enemies.Add(enemy);
			}
		}
	}
}