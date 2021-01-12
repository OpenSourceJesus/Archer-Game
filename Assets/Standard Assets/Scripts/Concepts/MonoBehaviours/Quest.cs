using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Extensions;

namespace ArcherGame
{
	public class Quest : MonoBehaviour, ISaveableAndLoadable
	{
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
		public TMP_Text nameText;
		public string description;
		public TMP_Text descriptionText;
		public Quest[] activateQuestsOnComplete;
		public Timer failTimer;
		[SaveAndLoadValue(false)]
		public bool isActive;
		[SaveAndLoadValue(false)]
		public bool isComplete;
		[SaveAndLoadValue(false)]
		public bool canBegin;
		public int moneyReward;
		public int skipsReward;
		public SpawnPoint startPoint;
		public Button beginButton;

		public virtual void Start ()
		{
			failTimer.onFinished += OnFail;
		}

		public virtual void Init ()
		{
			nameText.text = name;
			descriptionText.text = description;
			gameObject.SetActive(false);
		}

		public virtual void OnComplete ()
		{
			foreach (Quest quest in activateQuestsOnComplete)
			{
				if (!quest.isComplete)
					QuestManager.Instance.ActivateQuest (quest);
			}
			Player.instance.AddMoney (moneyReward);
			Player.instance.DisplayMoney ();
			SkipManager.skipPoints += skipsReward;
			isActive = false;
			GameManager.Instance.DeactivateGoForever (gameObject);
			// SaveAndLoadManager.SaveNonSharedData ();
			// SaveAndLoadManager.SaveSharedData ();
			SaveAndLoadManager.Instance.SaveToCurrentAccount ();
		}

		public virtual void OnFail (params object[] args)
		{
			QuestManager.Instance.ShowRetryScreen ();
		}

		public virtual void OnDestroy ()
		{
			failTimer.onFinished -= OnFail;
		}

		public virtual void OnActivate ()
		{
			QuestManager.activeQuests = QuestManager.activeQuests.Add(this);
			GameManager.Instance.ActivateGoForever (gameObject);
		}

		public virtual void Begin ()
		{
			if (!canBegin)
				return;
			if (startPoint != null)
				Player.instance.trs.position = Player.instance.unrotatedColliderRectOnOrigin.AnchorToPoint(startPoint.trs.position, startPoint.anchorPoint).center;
			failTimer.Reset ();
			failTimer.Start ();
			QuestManager.currentQuest = this;
		}
	}
}