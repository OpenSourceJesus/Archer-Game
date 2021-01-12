using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootLineAtPlayer : ShootWhereFacing
	{
		public int bulletCount;
		public float lineLength;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			spawner.up = Player.instance.trs.position - spawner.position;
			Bullet[] output = new Bullet[bulletCount];
			for (int i = 0; i < bulletCount; i ++)
				output[i] = ObjectPool.instance.SpawnComponent<Bullet>(bulletPrefab.prefabIndex, spawner.position, spawner.rotation);
			GameManager.Instance.StartCoroutine(TravelRoutine (spawner.position, spawner.up, output));
			return output;
		}

		public virtual IEnumerator TravelRoutine (Vector2 startPosition, Vector2 travelDirection, Bullet[] bullets)
		{
			List<BulletInLine> bulletsInLine = new List<BulletInLine>();
			LineSegment2D line = new LineSegment2D();
			Vector2 startDirection = travelDirection.Rotate(90);
			line.start = startPosition - (startDirection * lineLength / 2);
			line.end = startPosition + (startDirection * lineLength / 2);
			BulletInLine bulletInLine;
			for (int i = 0; i < bullets.Length; i ++)
			{
				Bullet bullet = bullets[i];
				float directedDistAlongLine = lineLength * ((float) i / lineLength);
				if (bulletCount % 2 == 0)
					directedDistAlongLine = lineLength * (((float) i + 0.5f) / lineLength);
				bulletInLine = new BulletInLine(bullet, line.GetPointWithDirectedDistance(directedDistAlongLine));
				bulletsInLine.Add(bulletInLine);
				bullet.Retarget (bulletInLine.destination - (Vector2) bullet.trs.position);
			}
			do
			{
				for (int i = 0; i < bulletsInLine.Count; i ++)
				{
					bulletInLine = bulletsInLine[i];
					if ((startPosition - (Vector2) bulletInLine.bullet.trs.position).sqrMagnitude >= (bulletInLine.destination - startPosition).sqrMagnitude)
					{
						bulletInLine.bullet.rigid.velocity = Vector2.zero;
						bulletInLine.bullet.trs.position = bulletInLine.destination;
						bulletInLine.bullet.trs.up = travelDirection;
						bulletsInLine.RemoveAt(i);
						i --;
					}
				}
				yield return new WaitForEndOfFrame();
			} while (bulletsInLine.Count > 0);
			for (int i = 0; i < bullets.Length; i ++)
				bullets[i].Retarget (travelDirection, true);
		}

		public struct BulletInLine
		{
			public Bullet bullet;
			public Vector2 destination;

			public BulletInLine (Bullet bullet, Vector2 destination)
			{
				this.bullet = bullet;
				this.destination = destination;
			}
		}
	}
}