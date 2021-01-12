using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class SetEnemyMoveMultiplier : MonoBehaviour, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Enemy enemy;
		public float moveMultiplier;

		void OnEnable ()
		{
			
		}

		public void DoUpdate ()
		{
			enemy.moveMultiplier = moveMultiplier;
		}

		void OnDisable ()
		{
			
		}
	}
}