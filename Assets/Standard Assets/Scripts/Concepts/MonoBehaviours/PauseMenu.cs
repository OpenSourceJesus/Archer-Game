using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Extensions;

namespace ArcherGame
{
	public class PauseMenu : SingletonMonoBehaviour<PauseMenu>
	{
		public Canvas canvas;
		public Button[] switchToSectionButtons;
		public Section section;

		public virtual void Show ()
		{
			if (PauseMenu.Instance != this)
			{
				PauseMenu.Instance.Show ();
				return;
			}
			gameObject.SetActive(true);
			GameManager.Instance.PauseGame (true);
			SetSection (section.GetHashCode());
			StartCoroutine(UpdateRoutine ());
		}

		int switchMenuSectionInput;
		int previousSwitchMenuSectionInput;
		public virtual IEnumerator UpdateRoutine ()
		{
			do
			{
				switchMenuSectionInput = InputManager.GetSwitchMenuSectionInput(MathfExtensions.NULL_INT);
				if (switchMenuSectionInput != 0 && previousSwitchMenuSectionInput != switchMenuSectionInput)
				{
					if (section.GetHashCode() + switchMenuSectionInput < 0)
						SetSection (Enum.GetValues(typeof(Section)).Length - 1);
					else if (section.GetHashCode() + switchMenuSectionInput >= Enum.GetValues(typeof(Section)).Length)
						SetSection (0);
					else
						SetSection (section.GetHashCode() + switchMenuSectionInput);
				}
				previousSwitchMenuSectionInput = switchMenuSectionInput;
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		public virtual void Hide ()
		{
			if (PauseMenu.Instance != this)
			{
				PauseMenu.Instance.Hide ();
				return;
			}
			gameObject.SetActive(false);
			if (section == Section.WorldMap)
				CloseWorldMap ();
			else if (section == Section.Quests)
				QuestManager.Instance.HideQuestsScreen ();
			else if (section == Section.Perks)
				// PerksMenu.Instance.gameObject.SetActive(false);
				PerksMenu.Instance.canvas.enabled = false;
			else
			{
				section = Section.WorldMap;
				canvas.enabled = true;
				AccountSelectMenu.Instance.gameObject.SetActive(false);
			}
			GameManager.Instance.PauseGame (false);
			StopCoroutine(UpdateRoutine ());
		}

		public virtual void SetSection (int index)
		{
			ColorBlock sectionButtonColors;
			foreach (Button _switchSectionButton in switchToSectionButtons)
			{
				sectionButtonColors = _switchSectionButton.colors;
				sectionButtonColors.colorMultiplier = 1;
				_switchSectionButton.colors = sectionButtonColors;
			}
			section = (Section) Enum.ToObject(typeof(Section), index);
			Button switchSectionButton = switchToSectionButtons[index];
			sectionButtonColors = switchSectionButton.colors;
			sectionButtonColors.colorMultiplier /= 2;
			switchSectionButton.colors = sectionButtonColors;
			if (section == Section.WorldMap)
				OpenWorldMap ();
			else
				CloseWorldMap ();
			if (section == Section.Quests)
				QuestManager.Instance.ShowQuestsScreen ();
			else
				QuestManager.Instance.HideQuestsScreen ();
			if (section == Section.Perks)
				// PerksMenu.Instance.gameObject.SetActive(true);
				PerksMenu.Instance.canvas.enabled = true;
			else
				// PerksMenu.Instance.gameObject.SetActive(false);
				PerksMenu.Instance.canvas.enabled = false;
			if (section == Section.MainMenu)
			{
				canvas.enabled = false;
				AccountSelectMenu.Instance.gameObject.SetActive(true);
			}
			else
			{
				canvas.enabled = true;
				AccountSelectMenu.Instance.gameObject.SetActive(false);
			}
		}
		
		public virtual void OpenWorldMap ()
		{
			WorldMap.Instance.Open ();
			canvas.worldCamera = WorldMap.Instance.worldMapCamera.camera;
		}

		public virtual void CloseWorldMap ()
		{
			WorldMap.Instance.Close ();
			canvas.worldCamera = GameCamera.Instance.camera;
		}

		public enum Section
		{
			WorldMap,
			Quests,
			Perks,
			MainMenu
		}
	}
}