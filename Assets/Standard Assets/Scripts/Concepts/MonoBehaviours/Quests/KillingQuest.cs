using UnityEngine;
using System;
using Extensions;

namespace ArcherGame
{
	public class KillingQuest : Quest
	{
		public Enemy[] enemiesToKill = new Enemy[0];
		Enemy[] enemiesToKillRemaining = new Enemy[0];
		public bool changePlayerHealth;
		public int playerHp;
		
		public override void Start ()
		{
			base.Start ();
			enemiesToKillRemaining = enemiesToKill;
			foreach (Enemy enemy in enemiesToKill)
				enemy.onDeath += delegate { OnDeath(enemy); };
		}

		public virtual void OnDeath (Enemy enemy)
		{
			enemiesToKillRemaining = enemiesToKillRemaining.Remove(enemy);
			if (enemiesToKillRemaining.Length == 0)
				isComplete = true;
		}

		public override void Begin ()
		{
			base.Begin ();
			if (!canBegin)
				return;
			if (changePlayerHealth)
			{
				Player.instance.Hp = playerHp;
				for (int i = playerHp; i < Player.instance.Hp; i ++)
					Destroy(Player.instance.lifeIconsParent.GetChild(0).gameObject);
			}
		}
	}
}