using UnityEngine;
using Extensions;
using ArcherGame;
using System;
using Random = UnityEngine.Random;

public class Spawner2D : MonoBehaviour
{
	public int prefabIndex;
	public int initSpawns;
	public SpawnZone2D[] spawnZones = new SpawnZone2D[0];
	public Timer spawnTimer;
	public float prefabRadius;
	public LayerMask whatICantSpawnIn;

	void Awake ()
	{
		for (int i = 0; i < initSpawns; i ++)
			Spawn ();
		spawnTimer.Reset ();
	}

	void OnEnable ()
	{
		spawnTimer.onFinished += Spawn;
		spawnTimer.Start ();
	}

	void Spawn (params object[] args)
	{
		Vector3 spawnPosition;
		do
		{
			SpawnZone2D spawnZone = spawnZones[Random.Range(0, spawnZones.Length)];
			spawnPosition = spawnZone.boxCollider.bounds.ToRect().RandomPoint();
			for (int i = 0; i < spawnZone.points.Length; i ++)
			{
				Transform point = spawnZone.points[i];
				if (!Physics2D.CircleCast(spawnPosition, prefabRadius, point.position - spawnPosition, (point.position - spawnPosition).magnitude, whatICantSpawnIn))
				{
					ObjectPool.Instance.SpawnComponent<ISpawnable>(prefabIndex, spawnPosition, Quaternion.LookRotation(Vector3.forward, Random.insideUnitCircle.normalized));
					return;
				}
			}
		} while (true);
	}

	void OnDisable ()
	{
		spawnTimer.Stop ();
		spawnTimer.onFinished -= Spawn;
	}

	[Serializable]
	public struct SpawnZone2D
	{
		public BoxCollider2D boxCollider;
		public Transform[] points;
	}
}