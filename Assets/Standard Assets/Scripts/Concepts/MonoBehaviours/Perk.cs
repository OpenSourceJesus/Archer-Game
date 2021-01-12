using UnityEngine;
using TMPro;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Perk : MonoBehaviour, ISaveableAndLoadable
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
		public static Perk[] instances = new Perk[0];
		public int cost;
		public TMP_Text costText;
		[SaveAndLoadValue(false)]
		public bool hasPurchased;

#if UNITY_EDITOR
		public virtual void Update ()
		{
			if (Application.isPlaying)
				return;
			costText.text = "" + cost;
		}
#endif

		public virtual void Init ()
		{
			if (hasPurchased)
				Apply ();
		}

		public virtual void Apply ()
		{
			for (int i = 0; i < instances.Length; i ++)
			{
				Perk perk = instances[i];
				perk.cost += PerksMenu.Instance.addToPerkCosts;
				perk.costText.text = "" + perk.cost;
			}
		}

		public virtual void Buy ()
		{
			if (!Obelisk.playerIsAtObelisk)
			{
				if (ArchivesManager.CurrentlyPlaying.CurrentMoney >= cost)
					GameManager.Instance.notificationText.text.text = "You must be at an obelisk to buy perks!";
				else
					GameManager.Instance.notificationText.text.text = "You must be at an obelisk to buy perks! You don't have enough money anyway!";
				GameManager.Instance.notificationText.Do ();
				return;
			}
			if (ArchivesManager.CurrentlyPlaying.CurrentMoney >= cost)
			{
				Player.instance.AddMoney (-cost);
				Player.instance.DisplayMoney ();
				hasPurchased = true;
				Apply ();
				// SaveAndLoadManager.SaveNonSharedData ();
				SaveAndLoadManager.Instance.SaveToCurrentAccount ();
			}
			else
			{
				GameManager.Instance.notificationText.text.text = "You don't have enough money to buy that!";
				GameManager.Instance.notificationText.Do ();
			}
		}
	}
}