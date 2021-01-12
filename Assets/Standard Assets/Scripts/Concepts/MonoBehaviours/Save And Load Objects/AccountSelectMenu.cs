using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class AccountSelectMenu : SaveAndLoadObject, ISaveableAndLoadable
	{
		public static AccountSelectMenu instance;
		public static AccountSelectMenu Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<AccountSelectMenu>();
				return instance;
			}
		}
		public AccountSelectMenuOption[] menuOptions;
		public CanvasGroup canvasGroup;

		public override void Setup ()
		{
			saveables = new ISaveableAndLoadable[0];
			foreach (AccountSelectMenuOption menuOption in menuOptions)
				saveables = saveables.Add(menuOption.account);
			base.Setup ();
			// print(saveEntries[0].memberEntries);
		}

		public static void Init ()
		{
			if (VirtualKeyboard.Instance != null)
				VirtualKeyboard.instance.DisableInput ();
			int numberOfEmptyAccounts = 0;
			AccountSelectMenuOption accountSelectMenuOption;
			Account account;
			for (int i = 0; i < AccountSelectMenu.Instance.menuOptions.Length; i ++)
			{
				accountSelectMenuOption = AccountSelectMenu.Instance.menuOptions[i];
				account = accountSelectMenuOption.account;
				if (string.IsNullOrEmpty(account.Name))
				{
					numberOfEmptyAccounts ++;
					accountSelectMenuOption.accountNameText.text = "Empty Account #" + numberOfEmptyAccounts;
				}
				else
				{
					accountSelectMenuOption.accountNameText.text = "Account: " + account.Name;
					accountSelectMenuOption.createButtonGo.SetActive(false);
					accountSelectMenuOption.playButtonGo.SetActive(true);
					accountSelectMenuOption.startCopyButtonGo.SetActive(true);
					accountSelectMenuOption.deleteButtonGo.SetActive(true);
				}
			}
		}

#if UNITY_EDITOR
		public virtual void Start ()
		{
			if (!Application.isPlaying)
			{
				menuOptions = GetComponentsInChildren<AccountSelectMenuOption>();
				return;
			}
		}
#endif
	}
}
