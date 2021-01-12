using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Extensions;
using TMPro;

namespace ArcherGame
{
	public class RacingMinigame : Minigame, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Timer timer;
		public TMP_Text timerText;
		public TMP_Text bestTimeText;
		public float timeRemainingIntoScoreMultiplier;
		bool isPlaying;

		public override void Begin ()
		{
			base.Begin ();
			bestTimeText.text = "Best time: " + (int) (highscore / timeRemainingIntoScoreMultiplier);
			GameManager.updatables = GameManager.updatables.Add(this);
			timer.Reset ();
			timer.Start ();
			GameManager.onGameScenesLoaded += delegate { RacingMinigame.Instance.ShowRetryScreen (); };
			isPlaying = true;
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DoUpdate ()
		{
			timerText.text = timer.TimeElapsed.ToString("0.0");
		}

		public override void Retry ()
		{
			GameManager.onGameScenesLoaded += delegate { RacingMinigame.Instance.MovePlayerToSpawnPoint (); };
			GameManager.Instance.LoadGameScenes ();
		}

		public virtual void End ()
		{
			timer.Stop ();
			DoUpdate ();
			GameManager.updatables = GameManager.updatables.Remove(this);
			if (isPlaying)
				AddScore ((int) (Mathf.Clamp(timer.timeRemaining, 0, timer.duration) * timeRemainingIntoScoreMultiplier));
			ShowRetryScreen ();
			isPlaying = false;
		}
	}
}