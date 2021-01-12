using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;
using UnityEngine;

namespace ArcherGame
{
	public class WavesMinigame : Minigame
	{
		public static WavesMinigame instance;
		public static WavesMinigame Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<WavesMinigame>();
				return instance;
			}
		}
		public int targetDifficulty;
		public int addToTargetDifficulty;
		public GameObject arenaGuardGo;
		public GameObject doorGo;
		int enemyCount;

		public override void Begin ()
		{
			if (WavesMinigame.Instance != this)
			{
				WavesMinigame.instance.Begin ();
				return;
			}
			arenaGuardGo.SetActive(false);
			doorGo.SetActive(true);
			base.Begin ();
			NextWave ();
			GameManager.onGameScenesLoaded += delegate { WavesMinigame.Instance.ShowRetryScreen (); };
		}

		public virtual void NextWave ()
		{
			if (WavesMinigame.Instance != this)
			{
				WavesMinigame.instance.NextWave ();
				return;
			}
			Player.instance.FullHeal ();
			int difficultyRemaining = targetDifficulty;
			List<EnemySpawnEntry> remainingEnemySpawnEntries = new List<EnemySpawnEntry>();
			remainingEnemySpawnEntries.AddRange(enemySpawnEntries);
			int randomIndex;
			EnemySpawnEntry enemySpawnEntry;
			do
			{
				randomIndex = Random.Range(0, remainingEnemySpawnEntries.Count);
				enemySpawnEntry = remainingEnemySpawnEntries[randomIndex];
				if (enemySpawnEntry.difficulty > difficultyRemaining)
					remainingEnemySpawnEntries.RemoveAt(randomIndex);
				else
				{
					StartCoroutine(SpawnEnemy (enemySpawnEntry));
					difficultyRemaining -= enemySpawnEntry.difficulty;
					enemyCount ++;
				}
			} while (remainingEnemySpawnEntries.Count > 0);
			targetDifficulty += addToTargetDifficulty;
		}

		public virtual IEnumerator SpawnEnemy (EnemySpawnEntry enemySpawnEntry)
		{
			CoroutineWithData cd = new CoroutineWithData(this, enemySpawnEntry.SpawnRoutine ());
			do
			{
				yield return cd.coroutine;
				Enemy enemy = cd.result as Enemy;
				if (enemy != null)
				{
					enemy.onDeath += OnEnemyDeath;
					yield break;
				}
			} while (true);
		}

		public virtual void OnEnemyDeath ()
		{
			enemyCount --;
			if (enemyCount == 0)
			{
				NextWave ();
				AddScore ();
			}
		}
	}
}