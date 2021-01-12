using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Extensions;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.EventSystems;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class AccountSelectMenuOption : MonoBehaviour
	{
		public Account account = new Account();
		public RectTransform rectTrs;
		public TMP_Text accountNameText;
		public static AccountSelectMenuOption copyAccountSelectMenuOption;
		public GameObject createButtonGo;
		public GameObject accountNameInputFieldGo;
		public GameObject playButtonGo;
		public GameObject startCopyButtonGo;
		public GameObject endCopyButtonGo;
		public GameObject cancelCopyButtonGo;
		public GameObject deleteButtonGo;
		public TemporaryActiveText tempActiveText;
		public CanvasGroup canvasGroup;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (rectTrs == null)
					rectTrs = GetComponent<RectTransform>();
				return;
			}
#endif
		}

		public virtual void StartCreation ()
		{
			AccountSelectMenu.Instance.canvasGroup.interactable = false;
			foreach (AccountSelectMenuOption menuOption in AccountSelectMenu.instance.menuOptions)
			{
				if (menuOption != this)
					menuOption.canvasGroup.interactable = false;
			}
			InputField accountNameInputField = accountNameInputFieldGo.GetComponent<InputField>();
			VirtualKeyboard.Instance.outputToInputField = accountNameInputField;
			VirtualKeyboard.Instance.doneKey.invokeOnPressed.RemoveAllListeners();
			VirtualKeyboard.Instance.doneKey.invokeOnPressed.AddListener(delegate { TryCreate (accountNameInputField.text); });
			VirtualKeyboard.Instance.cancelKey.invokeOnPressed.RemoveAllListeners();
			VirtualKeyboard.Instance.cancelKey.invokeOnPressed.AddListener(delegate { CancelCreate (); });
			VirtualKeyboard.Instance.EnableInput ();
			accountNameInputField.GetType().GetField("m_AllowInput", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(accountNameInputField, true);
			accountNameInputField.GetType().InvokeMember("SetCaretVisible", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, accountNameInputField, null);
			FindObjectOfType<EventSystem>().SetSelectedGameObject(accountNameInputField.gameObject);
		}

		public virtual void CancelCreate ()
		{
			AccountSelectMenu.Instance.canvasGroup.interactable = true;
			VirtualKeyboard.Instance.DisableInput ();
			foreach (AccountSelectMenuOption menuOption in AccountSelectMenu.Instance.menuOptions)
				menuOption.canvasGroup.interactable = true;
			accountNameInputFieldGo.SetActive(false);
			createButtonGo.SetActive(true);
		}

		public virtual void TryCreate (string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				tempActiveText.text.text = "Account name can't be empty!";
				tempActiveText.Do ();
				return;
			}
			foreach (Account account in ArchivesManager.Accounts)
			{
				if (account.Name == name)
				{
					tempActiveText.text.text = "Another account has that name!";
					tempActiveText.Do ();
					return;
				}
			}
			VirtualKeyboard.Instance.DisableInput ();
			accountNameText.text = "Account: " + name;
			account.Name = name;
			// SaveAndLoadManager.Instance.Save ();
			accountNameInputFieldGo.SetActive(false);
			playButtonGo.SetActive(true);
			startCopyButtonGo.SetActive(true);
			deleteButtonGo.SetActive(true);
			AccountSelectMenu.Instance.canvasGroup.interactable = true;
			foreach (AccountSelectMenuOption menuOption in AccountSelectMenu.Instance.menuOptions)
				menuOption.canvasGroup.interactable = true;
		}
		
		public virtual void Play ()
		{
			AccountSelectMenu.Instance.gameObject.SetActive(false);
			if (ArchivesManager.currentAccountIndex == account.index)
				PauseMenu.Instance.Hide ();
			else if (ArchivesManager.currentAccountIndex != -1)
			{
				SaveAndLoadManager.ResetPersistantValues ();
				ArchivesManager.currentAccountIndex = account.index;
				GameManager.Instance.LoadScene ("Init");
			}
			else
			{
				ArchivesManager.currentAccountIndex = account.index;
				SaveAndLoadManager.Instance.LoadFromCurrentAccount ();
				GameManager.Instance.PauseGame (false);
			}
		}

		public virtual void StartCopy ()
		{
			copyAccountSelectMenuOption = this;
			foreach (AccountSelectMenuOption accountSelectMenuOption in AccountSelectMenu.Instance.menuOptions)
			{
				if (accountSelectMenuOption != this)
				{
					accountSelectMenuOption.endCopyButtonGo.SetActive(true);
					if (string.IsNullOrEmpty(accountSelectMenuOption.account.Name))
					{
						accountSelectMenuOption.createButtonGo.SetActive(false);
						accountSelectMenuOption.accountNameInputFieldGo.SetActive(false);
					}
					else
					{
						accountSelectMenuOption.playButtonGo.SetActive(false);
						accountSelectMenuOption.startCopyButtonGo.SetActive(false);
						accountSelectMenuOption.deleteButtonGo.SetActive(false);
					}
				}
			}
		}

		public virtual void CancelCopy ()
		{
			foreach (AccountSelectMenuOption accountSelectMenuOption in AccountSelectMenu.Instance.menuOptions)
			{
				if (accountSelectMenuOption != this)
				{
					accountSelectMenuOption.endCopyButtonGo.SetActive(false);
					if (string.IsNullOrEmpty(accountSelectMenuOption.account.Name))
						accountSelectMenuOption.createButtonGo.SetActive(true);
					else
					{
						accountSelectMenuOption.playButtonGo.SetActive(true);
						accountSelectMenuOption.startCopyButtonGo.SetActive(true);
						accountSelectMenuOption.deleteButtonGo.SetActive(true);
					}
				}
			}
		}

		public virtual void EndCopy ()
		{
			account = copyAccountSelectMenuOption.account;
			account.Name += " (Copy)";
			accountNameText.text = "Account: " + account.Name;
			account.index = rectTrs.GetSiblingIndex();
			// SaveAndLoadManager.Instance.Save ();
			copyAccountSelectMenuOption.cancelCopyButtonGo.SetActive(false);
			foreach (AccountSelectMenuOption accountSelectMenuOption in AccountSelectMenu.Instance.menuOptions)
			{
				if (accountSelectMenuOption != this)
				{
					accountSelectMenuOption.endCopyButtonGo.SetActive(false);
					if (string.IsNullOrEmpty(accountSelectMenuOption.account.Name))
						accountSelectMenuOption.createButtonGo.SetActive(true);
					else
					{
						accountSelectMenuOption.playButtonGo.SetActive(true);
						accountSelectMenuOption.startCopyButtonGo.SetActive(true);
						accountSelectMenuOption.deleteButtonGo.SetActive(true);
					}
				}
			}
		}

		public virtual void Delete ()
		{
			SaveAndLoadManager.Instance.Delete (account.index);
			account = account.GetDefault();
			account.index = rectTrs.GetSiblingIndex();
			// SaveAndLoadManager.Instance.Save ();
			if (ArchivesManager.currentAccountIndex == account.index)
			{
				// ArchivesManager.currentAccountIndex = -1;
				// SaveAndLoadManager.ResetPersistantValues ();
				GameManager.Instance.LoadGameScenes ();
			}
			AccountSelectMenu.Init ();
		}
	}
}
