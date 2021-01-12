using UnityEngine;
using ArcherGame;
using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	public class QuestManager : SingletonMonoBehaviour<QuestManager>
	{
		public GameObject retryQuestScreen;
		public static Quest currentQuest;
		public static Quest[] activeQuests = new Quest[0];
		public GameObject questsScreen;
		public Quest[] quests = new Quest[0];

		public virtual void Start ()
		{
			foreach (Quest quest in quests)
				quest.Init ();
		}

		public virtual void RetryCurrentQuest ()
		{
			if (QuestManager.Instance != this)
			{
				QuestManager.Instance.RetryCurrentQuest ();
				return;
			}
			retryQuestScreen.SetActive(false);
			currentQuest.Begin ();
		}
		
		public virtual void StopCurrentQuest ()
		{
			if (QuestManager.Instance != this)
			{
				QuestManager.Instance.StopCurrentQuest ();
				return;
			}
			currentQuest = null;
			retryQuestScreen.SetActive(false);
			GameManager.onGameScenesLoaded -= delegate { QuestManager.Instance.ShowRetryScreen (); };
		}

		public virtual void ShowRetryScreen ()
		{
			if (QuestManager.Instance != this)
			{
				QuestManager.Instance.ShowRetryScreen ();
				return;
			}
			retryQuestScreen.SetActive(true);
		}

		public virtual void ActivateQuest (Quest quest)
		{
			quest.isActive = true;
			quest.OnActivate ();
		}

		public virtual void CompleteQuest (Quest quest)
		{
			quest.isComplete = true;
			quest.OnComplete ();
		}

		public virtual void FailQuest (Quest quest)
		{
			quest.OnFail ();
		}

		public virtual void ShowQuestsScreen ()
		{
			if (QuestManager.Instance != this)
			{
				QuestManager.Instance.ShowQuestsScreen ();
				return;
			}
			questsScreen.SetActive(true);
		}

		public virtual void HideQuestsScreen ()
		{
			if (QuestManager.Instance != this)
			{
				QuestManager.Instance.HideQuestsScreen ();
				return;
			}
			questsScreen.SetActive(false);
		}
	}
}