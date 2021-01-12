using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ThrowBombsAtPlayer : ShootWhereFacing
	{
		public float throwRange;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			spawner.up = Player.instance.trs.position - spawner.position;
			Bullet[] output = new Bullet[] {  ObjectPool.instance.SpawnComponent<Bullet>(bulletPrefab.prefabIndex, spawner.position, Quaternion.LookRotation(Vector3.forward, spawner.up)) };
			List<Bomb> bombs = new List<Bomb>();
			Bomb bomb;
			foreach (Bullet bullet in output)
			{
				bomb = bullet as Bomb;
				if (bomb != null)
				{
					bomb.startPoint = spawner.position;
					bomb.destination = spawner.position + Vector3.ClampMagnitude(Player.instance.trs.position - spawner.position, throwRange);
					bombs.Add(bomb);
				}
			}
			GameManager.Instance.StartCoroutine(TravelRoutine (bombs));
			return output;
		}

		public virtual IEnumerator TravelRoutine (List<Bomb> bombs)
		{
			do
			{
				for (int i = 0; i < bombs.Count; i ++)
				{
					Bomb bomb = bombs[i];
					if (bomb.gameObject.activeSelf)
					{
						if ((bomb.startPoint - (Vector2) bomb.trs.position).sqrMagnitude >= (bomb.startPoint - bomb.destination).sqrMagnitude)
						{
							bomb.rigid.velocity = Vector2.zero;
							bomb.trs.position = bomb.destination;
							bomb.anim.Play("Explode Delay");
							bombs.RemoveAt(i);
							i --;
							if (bombs.Count == 0)
								yield break;
						}
					}
					else
					{
						bombs.RemoveAt(i);
						i --;
						if (bombs.Count == 0)
							yield break;
					}
				}
				yield return new WaitForEndOfFrame();
			} while (true);
		}
	}
}