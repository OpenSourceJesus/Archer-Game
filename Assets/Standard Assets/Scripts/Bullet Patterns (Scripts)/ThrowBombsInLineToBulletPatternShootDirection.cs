using UnityEngine;
using System.Collections.Generic;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ThrowBombsInLineToBulletPatternShootDirection : ThrowBombToBulletPatternShootDirection
	{
		public float[] normalizedDistanceToThrowPositions = new float[0];

		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			Bullet[] output = new Bullet[normalizedDistanceToThrowPositions.Length];
			Vector2 shootDirection = bulletPattern.GetShootDirection(spawner);
			LineSegment2D lineSegment = new LineSegment2D(spawner.position, (Vector2) spawner.position + (shootDirection * throwRange));
			List<BombEntry> bombEntries = new List<BombEntry>();
			for (int i = 0; i < normalizedDistanceToThrowPositions.Length; i ++)
			{
				Bullet bullet = ObjectPool.instance.SpawnComponent<Bullet>(bulletPrefab.prefabIndex, spawner.position, Quaternion.LookRotation(Vector3.forward, shootDirection));
				Bomb bomb = bullet as Bomb;
				if (bomb != null)
				{
					float directedDistanceAlongLine = throwRange * normalizedDistanceToThrowPositions[i];
					BombEntry bombEntry = new BombEntry();
					bombEntry.bomb = bomb;
					bombEntry.startPoint = spawner.position;
					bombEntry.destination = lineSegment.GetPointWithDirectedDistance(directedDistanceAlongLine);
					bombEntries.Add(bombEntry);
					output[i] = bomb;
				}
			}
			GameManager.Instance.StartCoroutine(TravelRoutine (bombEntries));
			return output;
		}
	}
}