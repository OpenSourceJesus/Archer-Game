using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;
using TMPro;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Minigame : SingletonMonoBehaviour<Minigame>, ISaveableAndLoadable
	{
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		[HideInInspector]
		public int score;
		[SaveAndLoadValue(false)]
		public int highscore;
		public EnemySpawnEntry[] enemySpawnEntries = new EnemySpawnEntry[0];
		public EnemySpawnPoint[] enemySpawnPoints = new EnemySpawnPoint[0];
		public GameObject scorePanelGo;
		public GameObject retryScreenGo;
		public TMP_Text scoreText;
		public TMP_Text highscoreText;
#if UNITY_EDITOR
		public bool update;
#endif
		public SpawnPoint playerSpawnPoint;
		public float scoreIntoMoney;

		public virtual IEnumerator Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				yield break;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			yield return new WaitForEndOfFrame();
			yield return new WaitUntil(() => (!SaveAndLoadManager.isLoading));
			if (scoreText != null)
				scoreText.text += score;
			if (highscoreText != null)
				highscoreText.text += highscore;
		}

#if UNITY_EDITOR
		public virtual void DoEditorUpdate ()
		{
			if (!update)
				return;
			update = false;
			foreach (EnemySpawnEntry enemySpawnEntry in enemySpawnEntries)
			{
				enemySpawnEntry.enemySpawnPoints = new EnemySpawnPoint[0];
				foreach (EnemySpawnPoint enemySpawnPoint in enemySpawnPoints)
				{
					if (enemySpawnPoint.enemiesThatUseMe.Contains(enemySpawnEntry.enemyPrefab))
						enemySpawnEntry.enemySpawnPoints = enemySpawnEntry.enemySpawnPoints.Add(enemySpawnPoint);
				}
			}
		}
#endif

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorApplication.update -= DoEditorUpdate;
				return;
			}
#endif
		}

		public virtual void Begin ()
		{
			scorePanelGo.SetActive(true);
		}

		public virtual void AddScore (int amount = 1)
		{
			score += amount;
			if (scoreText != null)
				scoreText.text = scoreText.text.Replace("" + (score - amount), "" + score);
			if (score > highscore)
			{
				if (highscoreText != null)
					highscoreText.text = highscoreText.text.Replace("" + highscore, "" + score);
				Player.instance.AddMoney ((int) ((score - highscore) * scoreIntoMoney));
				Player.instance.DisplayMoney ();
				highscore = score;
				// SaveAndLoadManager.SaveSharedData ();
				// SaveAndLoadManager.SaveNonSharedData ();
				SaveAndLoadManager.Instance.SaveToCurrentAccount ();
			}
		}

		public virtual void Retry ()
		{
			MovePlayerToSpawnPoint ();
			Begin ();
		}

		public virtual void MovePlayerToSpawnPoint ()
		{
			Player.instance.trs.position = Player.instance.unrotatedColliderRectOnOrigin.AnchorToPoint(playerSpawnPoint.trs.position, playerSpawnPoint.anchorPoint).center;
		}

		public virtual void ShowRetryScreen ()
		{
			retryScreenGo.SetActive(true);
		}

		[Serializable]
		public class EnemySpawnEntry
		{
			public Enemy enemyPrefab;
			public EnemySpawnPoint[] enemySpawnPoints = new EnemySpawnPoint[0];
			public int difficulty;

			public virtual IEnumerator SpawnRoutine ()
			{
				EnemySpawnPoint enemySpawnPoint;
				Rect anchoredEnemyColliderRect;
				do
				{
					enemySpawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
					yield return new WaitForEndOfFrame();
					anchoredEnemyColliderRect = enemyPrefab.collider.GetRect(enemyPrefab.trs).AnchorToPoint(enemySpawnPoint.trs.position, enemySpawnPoint.anchorPoint);
				} while (Physics2D.OverlapArea(anchoredEnemyColliderRect.min, anchoredEnemyColliderRect.max, Physics2D.GetLayerCollisionMask(enemyPrefab.gameObject.layer)) != null || new Circle2D(Player.instance.trs.position, enemySpawnPoint.minSpawnRangeFromPlayer).DoIIntersectWithRect(anchoredEnemyColliderRect));
				Enemy enemy;
				do
				{
					enemy = ObjectPool.instance.SpawnComponent<Enemy>(enemyPrefab.prefabIndex, anchoredEnemyColliderRect.center);
				} while (enemy == null);
				enemy.Start ();
				AwakableEnemy awakableEnemy = enemy as AwakableEnemy;
				if (awakableEnemy != null)
				{
					awakableEnemy.Awaken ();
					awakableEnemy.invulnerable = false;
				}
				yield return enemy;
			}
		}
	}
}