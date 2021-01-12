using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;
using System;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;
using PlayerIOClient;

namespace ArcherGame
{
	//[ExecuteAlways]
	public class ArchivesManager : SingletonMonoBehaviour<ArchivesManager>, ISaveableAndLoadable
	{
		public const string EMPTY_ACCOUNT_INDICATOR = "␀";
		public const int MAX_ACCOUNTS = 5;
		public const string VALUE_SEPARATOR = "⧫";
		[SaveAndLoadValue(true)]
		public static string[] localAccountNames = new string[0];
		[SaveAndLoadValue(true)]
		public static string[] localAccountPasswords = new string[0];
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
		public Account[] localAccountsData = new Account[0];
		public Account newAccount;
		public static Account player1Account;
		public static Account player2Account;
		public static Account activeAccount;
		// public MenuOption addAccountOption;
		// public MenuOption[] accountOptions = new MenuOption[0];
		// public MenuOption[] viewInfoMenuOptions = new MenuOption[0];
		public static string ActivePlayerUsername
		{
			get
			{
				if (activeAccount == null)
					return EMPTY_ACCOUNT_INDICATOR;
				else
					return activeAccount.username;
			}
		}
		public GameObject deleteAccountScreen;
		public Text deleteAccountText;
		public static int indexOfCurrentAccountToDelete;
		public static Account currentAccountToDelete;
		public GameObject accountInfoScreen;
		public Text accountInfoTitleText;
		public Text accountInfoContentText;
		public static Account currentAccountToViewInfo;
		public Scrollbar accountInfoScrollbar;
		// public static MenuOption player1AccountAssigner;
		// public static MenuOption player2AccountAssigner;
		public Transform trs;
		public static Account[] Accounts
		{
			get
			{
				List<Account> output = new List<Account>();
				foreach (AccountSelectMenuOption accountSelectMenuOption in AccountSelectMenu.Instance.menuOptions)
					output.Add(accountSelectMenuOption.account);
				return output.ToArray();
			}
		}
		public static Account CurrentlyPlaying
		{
			get
			{
				if (currentAccountIndex == -1)
					return null;
				return Accounts[currentAccountIndex];
			}
		}
		public static int currentAccountIndex = -1;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (ArchivesManager.Instance != null && ArchivesManager.instance != this)
			{
				UpdateMenus ();
				// ArchivesManager.instance.addAccountOption = addAccountOption;
				// ArchivesManager.instance.accountOptions = accountOptions;
				// ArchivesManager.instance.viewInfoMenuOptions = viewInfoMenuOptions;
				ArchivesManager.instance.deleteAccountScreen = deleteAccountScreen;
				ArchivesManager.instance.deleteAccountText = deleteAccountText;
				ArchivesManager.instance.accountInfoScreen = accountInfoScreen;
				ArchivesManager.instance.accountInfoTitleText = accountInfoTitleText;
				ArchivesManager.instance.accountInfoContentText = accountInfoContentText;
				ArchivesManager.instance.accountInfoScrollbar = accountInfoScrollbar;
				return;
			}
			trs.SetParent(null);
			base.Awake ();
			// if (BuildManager.IsFirstStartup)
			// {
			// 	if (BuildManager.Instance.clearDataOnFirstStartup)
			// 	{
			// 		SaveAndLoadManager.data.Clear();
			// 		if (SaveAndLoadManager.Instance.usePlayerPrefs)
			// 			PlayerPrefs.DeleteAll();
			// 		else
			// 			File.Delete(SaveAndLoadManager.Instance.saveFileFullPath);
			// 	}
			// 	else
			// 		SaveAndLoadManager.Instance.Load ();
			// 	BuildManager.IsFirstStartup = false;
			// }
			// else
			// 	SaveAndLoadManager.Instance.Load ();
			Connect ();
		}

		public virtual void Connect ()
		{
			NetworkManager.Instance.Connect (OnAuthenticateSucess, OnAuthenticateFail);
		}

		public virtual void OnAuthenticateSucess (Client client)
		{
			Debug.Log("OnAuthenticateSucess");
			NetworkManager.client = client;
		}

		public virtual void OnAuthenticateFail (PlayerIOError error)
		{
			Debug.Log("OnAuthenticateFail: " + error.ToString());
			// Connect ();
		}

		public virtual void UpdateMenus ()
		{
			// if (accountOptions[0] == null)
			// 	return;
			// MenuOption accountOption;
			// for (int i = 0; i < MAX_ACCOUNTS; i ++)
			// {
			// 	if (LocalAccountNames.Length > i)
			// 	{
			// 		accountOption = accountOptions[i];
			// 		accountOption.enabled = true;
			// 		accountOption.textMesh.text = LocalAccountNames[i];
			// 		accountOption = viewInfoMenuOptions[i];
			// 		accountOption.enabled = true;
			// 		accountOption.textMesh.text = LocalAccountNames[i];
			// 	}
			// 	else
			// 	{
			// 		accountOption = accountOptions[i];
			// 		accountOption.enabled = false;
			// 		accountOption.textMesh.text = "Account " + (i + 1);
			// 		accountOption = viewInfoMenuOptions[i];
			// 		accountOption.enabled = false;
			// 		accountOption.textMesh.text = "Account " + (i + 1);
			// 	}
			// 	accountOptions[i].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = true;
			// 	accountOptions[i].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = true;
			// }
			// if (player1Account != null)
			// 	accountOptions[ArchivesManager.Instance.localAccountsData.IndexOf(player1Account)].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = false;
			// if (player2Account != null)
			// 	accountOptions[ArchivesManager.Instance.localAccountsData.IndexOf(player2Account)].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = false;
			// addAccountOption.enabled = LocalAccountNames.Length < MAX_ACCOUNTS;
		}
		
		public virtual void StartContinuousContinuousScrollAccountInfo (float velocity)
		{
			if (ArchivesManager.Instance != this)
			{
				ArchivesManager.Instance.StartContinuousContinuousScrollAccountInfo (velocity);
				return;
			}
			StartCoroutine(ContinuousScrollAccountInfoRoutine (velocity));
		}

		public virtual IEnumerator ContinuousScrollAccountInfoRoutine (float velocity)
		{
			while (true)
			{
				ScrollAccountInfo (velocity);
				yield return new WaitForEndOfFrame();
			}
		}

		public virtual void ScrollAccountInfo (float velocity)
		{
			accountInfoScrollbar.value += velocity * Time.deltaTime;
		}

		public virtual void EndContinuousContinuousScrollAccountInfo ()
		{
			if (ArchivesManager.Instance != this)
			{
				ArchivesManager.Instance.EndContinuousContinuousScrollAccountInfo ();
				return;
			}
			StopAllCoroutines();
		}

		public virtual void TryToSetNewAccountUsername ()
		{
			if (ArchivesManager.Instance != this)
			{
				ArchivesManager.Instance.TryToSetNewAccountUsername ();
				return;
			}
			VirtualKeyboard.Instance.DisableInput ();
			NetworkManager.Instance.notificationTextObject.obj.SetActive(true);
			NetworkManager.Instance.notificationTextObject.text.text = "Loading...";
			string username = VirtualKeyboard.Instance.outputToInputField.text;
			newAccount.username = username.Replace(" ", "");
			if (newAccount.username.Length == 0)
			{
				NetworkManager.Instance.notificationTextObject.text.text = "The username must contain at least one non-space character";
				NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
				VirtualKeyboard.Instance.EnableInput ();
				return;
			}
			NetworkManager.client.BigDB.LoadMyPlayerObject(
				delegate (DatabaseObject dbObject)
				{
					if (dbObject.Count > 0)
					{
						NetworkManager.Instance.notificationTextObject.text.text = "The username can't be used. It has already been registered online by someone else.";
						NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
						VirtualKeyboard.Instance.EnableInput ();
						return;
					}
					else
					{
						NetworkManager.Instance.notificationTextObject.obj.SetActive(false);
						VirtualKeyboard.Instance.trs.parent.gameObject.SetActive(false);
						VirtualKeyboard.Instance.EnableInput ();
						NetworkManager.client.BigDB.LoadOrCreate("PlayerObjects", username, OnNewAccountDBObjectCreateSuccess, OnNewAccountDBObjectCreateFail);
					}
				},
				delegate (PlayerIOError error)
				{
					NetworkManager.Instance.notificationTextObject.text.text = "Error: " + error.ToString();
					NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
					VirtualKeyboard.Instance.EnableInput ();
				}
			);
		}

		public virtual void OnNewAccountDBObjectCreateSuccess (DatabaseObject dbObject)
		{
			newAccount.username = dbObject.Key;
			VirtualKeyboard.Instance.trs.parent.parent.GetChild(1).gameObject.SetActive(true);
			VirtualKeyboard.Instance.EnableInput ();
		}

		public virtual void OnNewAccountDBObjectCreateFail (PlayerIOError error)
		{
			NetworkManager.Instance.notificationTextObject.text.text = "Error: " + error.ToString();
			NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
			VirtualKeyboard.Instance.EnableInput ();
		}

		public virtual void TryToSetNewAccountPassword ()
		{
			if (ArchivesManager.Instance != this)
			{
				ArchivesManager.Instance.TryToSetNewAccountPassword ();
				return;
			}
			VirtualKeyboard.Instance.DisableInput ();
			NetworkManager.Instance.notificationTextObject.obj.SetActive(true);
			NetworkManager.Instance.notificationTextObject.text.text = "Loading...";
			newAccount.password = VirtualKeyboard.Instance.outputToInputField.text;
			NetworkManager.client.BigDB.LoadMyPlayerObject(
				delegate (DatabaseObject dbObject)
				{
					if (dbObject.Count > 0)
					{
						NetworkManager.Instance.notificationTextObject.text.text = "The username previously chosen can't be used. It has already been registered online by someone else.";
						NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
						VirtualKeyboard.Instance.EnableInput ();
						return;
					}
					else
					{
						dbObject.Set("password", newAccount.password);
						dbObject.Save(true, false, OnNewAccountDBObjectSaveSuccess, OnNewAccountDBObjectSaveFail);
					}
				},
				delegate (PlayerIOError error)
				{
					NetworkManager.Instance.notificationTextObject.text.text = "Error: " + error.ToString();
					NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
					VirtualKeyboard.Instance.EnableInput ();
				}
			);
		}

		public virtual void OnNewAccountDBObjectSaveSuccess ()
		{
			NetworkManager.Instance.notificationTextObject.obj.SetActive(false);
			VirtualKeyboard.Instance.trs.parent.gameObject.SetActive(false);
			VirtualKeyboard.instance.EnableInput ();
			localAccountsData[localAccountNames.Length].username = newAccount.username;
			localAccountsData[localAccountNames.Length].password = newAccount.password;
			foreach (Account Account in localAccountsData)
				Account.UpdateData ();
			localAccountNames = localAccountNames.Add(newAccount.username);
			UpdateMenus ();
		}

		public virtual void OnNewAccountDBObjectSaveFail (PlayerIOError error)
		{
			NetworkManager.Instance.notificationTextObject.text.text = "Error: " + error.ToString();
			NetworkManager.instance.StartCoroutine(NetworkManager.instance.notificationTextObject.DoRoutine ());
			VirtualKeyboard.Instance.EnableInput ();
		}

		public virtual void DeleteAccount ()
		{
			if (ArchivesManager.Instance != this)
			{
				ArchivesManager.instance.DeleteAccount ();
				return;
			}
			NetworkManager.client.BigDB.DeleteKeys("PlayerObjects", new string[1] { localAccountNames[indexOfCurrentAccountToDelete] }, OnDelteAccountDBObjectSuccess, OnDeleteAccountDBObjectFail);
		}

		public virtual void OnDelteAccountDBObjectSuccess ()
		{
			// foreach (string key in SaveAndLoadManager.data.Keys)
			// {
			// 	if (key.StartsWith(LocalAccountNames[indexOfCurrentAccountToDelete] + VALUE_SEPARATOR))
			// 		SaveAndLoadManager.data.Remove(key);
			// }
			localAccountsData[indexOfCurrentAccountToDelete].Reset ();
			localAccountNames = localAccountNames.RemoveAt(indexOfCurrentAccountToDelete);
			localAccountPasswords = localAccountPasswords.RemoveAt(indexOfCurrentAccountToDelete);
			foreach (Account Account in localAccountsData)
				Account.UpdateData ();
			if (activeAccount == localAccountsData[indexOfCurrentAccountToDelete])
				activeAccount = null;
			if (player1Account == localAccountsData[indexOfCurrentAccountToDelete])
				player1Account = null;
			else if (player2Account == localAccountsData[indexOfCurrentAccountToDelete])
				player2Account = null;
			SaveAndLoadManager.Instance.SaveToCurrentAccount ();
			UpdateMenus ();
		}

		public virtual void OnDeleteAccountDBObjectFail (PlayerIOError error)
		{
			NetworkManager.Instance.notificationTextObject.text.text = "Error: " + error.ToString();
			NetworkManager.instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DoRoutine ());
		}

		public virtual void UpdateAccount (Account Account)
		{
			if (ArchivesManager.Instance != this)
			{
				ArchivesManager.instance.UpdateAccount (Account);
				return;
			}
		}
	}
}
