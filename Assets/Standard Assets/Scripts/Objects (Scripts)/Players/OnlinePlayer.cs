using UnityEngine;
using UnityEngine.UI;
using Extensions;

namespace ArcherGame
{
	[ExecuteInEditMode]
	public class OnlinePlayer : Player, ISpawnable
	{
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public int playerId;
		public MonoBehaviour[] disableMonoBehavioursIfIAmToo;
		public uint score;
		public float scoreTextHeight;
		public RectTransform scoreTextRectTrs;
		public Text scoreText;
		public static OnlinePlayer localPlayer;
		[HideInInspector]
		public float minMoveDistToSyncSqr;
		public float minMoveDistToSync;
        public Team owner;
		Vector2 syncedPosition = VectorExtensions.NULL2;
		Vector2 weaponSyncedPosition = VectorExtensions.NULL2;

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			scoreTextRectTrs.position = trs.position + Vector3.up * scoreTextHeight;
		}

#if UNITY_EDITOR
		void OnValidate ()
		{
			minMoveDistToSyncSqr = minMoveDistToSync * minMoveDistToSync;
		}
#endif

		public override void DoUpdate ()
		{
            base.DoUpdate ();
			if (((Vector2) trs.position - syncedPosition).sqrMagnitude >= minMoveDistToSyncSqr)
			{
				syncedPosition = trs.position;
				NetworkManager.connection.Send("Move Transform", true, syncedPosition.x, syncedPosition.y);
			}
		}

		public virtual void OnDeath (OnlinePlayer killer, OnlinePlayer victim)
		{
			if (killer != null)
			{
				if (OnlineGameMode.localPlayer.playerId == killer.playerId)
				{
					killer.ChangeScore ((uint) Mathf.Clamp(victim.score, 1, int.MaxValue));
					SendChangeScoreMessage ((uint) Mathf.Clamp(score * OnlineGameMode.Instance.bountyMultiplier, 1, int.MaxValue));
				}
			}
			if (OnlineGameMode.localPlayer == this)
			{
				if (ArchivesManager.player1Account != null)
				{
					ArchivesManager.player1Account.onlineData.deaths ++;
					ArchivesManager.player1Account.onlineData.killDeathRatio = ArchivesManager.player1Account.onlineData.kills / ArchivesManager.player1Account.onlineData.deaths;
					ArchivesManager.Instance.UpdateAccount (ArchivesManager.player1Account);
				}
				GameManager.Instance.ReloadActiveScene ();
			}
		}

		public virtual void SendChangeScoreMessage (uint amount)
		{
			NetworkManager.connection.Send("Change Score", amount);
		}

		public virtual void ChangeScore (uint amount)
		{
			score += amount;
			if (OnlineGameMode.localPlayer == this)
			{
				scoreText.text = "Score: " + score;
				if (ArchivesManager.player1Account != null)
				{
					ArchivesManager.player1Account.onlineData.kills ++;
					ArchivesManager.player1Account.onlineData.killDeathRatio = ArchivesManager.player1Account.onlineData.kills / ArchivesManager.player1Account.onlineData.deaths;
					if (score > ArchivesManager.player1Account.onlineData.highscore)
						ArchivesManager.player1Account.onlineData.highscore = score;
					ArchivesManager.Instance.UpdateAccount (ArchivesManager.player1Account);
				}
			}
			else
				scoreText.text = "Bounty: " + Mathf.Clamp(score * OnlineGameMode.Instance.bountyMultiplier, 1, int.MaxValue);
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
            base.OnDisable ();
			foreach (MonoBehaviour monoBehaviour in disableMonoBehavioursIfIAmToo)
				monoBehaviour.enabled = false;
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (OnlineGameMode.localPlayer == this)
				GameManager.updatables = GameManager.updatables.Remove(this);
			else
				OnlineGameMode.nonLocalPlayers = OnlineGameMode.nonLocalPlayers.Remove(this);
		}
	}
}