using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ThrowBombToBulletPatternShootDirection : BulletPattern
	{
		public BulletPattern bulletPattern;
		public float throwRange;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			Vector2 shootDirection = bulletPattern.GetShootDirection(spawner);
			Bullet[] output = new Bullet[] {  ObjectPool.instance.SpawnComponent<Bullet>(bulletPrefab.prefabIndex, spawner.position, Quaternion.LookRotation(Vector3.forward, shootDirection)) };
			List<BombEntry> bombEntries = new List<BombEntry>();
			Bomb bomb;
			foreach (Bullet bullet in output)
			{
				bomb = bullet as Bomb;
				if (bomb != null)
				{
					BombEntry bombEntry = new BombEntry();
					bombEntry.bomb = bomb;
					bombEntry.startPoint = spawner.position;
					bombEntry.destination = spawner.position + Vector3.ClampMagnitude(shootDirection, throwRange);
					bombEntries.Add(bombEntry);
				}
			}
			GameManager.Instance.StartCoroutine(TravelRoutine (bombEntries));
			return output;
		}

		public virtual IEnumerator TravelRoutine (List<BombEntry> bombEntries)
		{
			BombEntry bombEntry;
			do
			{
				for (int i = 0; i < bombEntries.Count; i ++)
				{
					bombEntry = bombEntries[i];
					Bomb bomb = bombEntry.bomb;
					if (bomb.gameObject.activeSelf)
					{
						if ((bombEntry.startPoint - (Vector2) bomb.trs.position).sqrMagnitude >= (bombEntry.startPoint - bombEntry.destination).sqrMagnitude)
						{
							bomb.rigid.velocity = Vector2.zero;
							bomb.trs.position = bombEntry.destination;
							bomb.anim.Play("Explode Delay");
							bombEntries.RemoveAt(i);
							i --;
							if (bombEntries.Count == 0)
								yield break;
						}
					}
					else
					{
						bombEntries.RemoveAt(i);
						i --;
						if (bombEntries.Count == 0)
							yield break;
					}
				}
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		public struct BombEntry
		{
			public Bomb bomb;
			public Vector2 startPoint;
			public Vector2 destination;
		}
	}
}