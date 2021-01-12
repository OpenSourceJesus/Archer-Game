using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class ShootBulletPatternThenConstantlyRotateShotsTowardsPlayer : BulletPattern
	{
		public BulletPattern bulletPattern;
		[MakeConfigurable]
		public float rotateRate;
		
		public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
		{
			Bullet[] output = bulletPattern.Shoot (spawner, bulletPrefab);
			GameManager.Instance.StartCoroutine(ConstantlyRotateShotsTowardsPlayerRoutine (output));
			return output;
		}

		public virtual IEnumerator ConstantlyRotateShotsTowardsPlayerRoutine (Bullet[] bullets)
		{
			Bullet bullet;
			do
			{
				for (int i = 0; i < bullets.Length; i ++)
				{
					bullet = bullets[i];
					if (bullet.gameObject.activeSelf)
					{
						bullet.rigid.velocity = bullet.rigid.velocity.RotateTo(Player.instance.trs.position - bullet.trs.position, rotateRate * Time.deltaTime);
						if (bullet.rigid.velocity.magnitude < bullet.moveSpeed)
							bullet.rigid.velocity = bullet.rigid.velocity.normalized * bullet.moveSpeed;
						bullet.trs.up = bullet.rigid.velocity;
					}
					else
					{
						bullets = bullets.RemoveAt(i);
						i --;
						if (bullets.Length == 0)
							yield break;
					}
				}
				yield return new WaitForEndOfFrame();
			} while (true);
		}
	}
}