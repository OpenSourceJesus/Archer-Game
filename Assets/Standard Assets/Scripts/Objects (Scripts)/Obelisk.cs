using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Obelisk : SpawnPoint, IUpdatable, ISaveableAndLoadable
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
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		[SaveAndLoadValue(false)]
		public bool found;
		public static Obelisk[] instances = new Obelisk[0];
		public WorldMapIcon worldMapIcon;
		public GameObject foundIndicator;
		public static bool playerIsAtObelisk;
		public static bool playerJustFastTraveled;
		public static string nameOfLastObeliskPlayerWasAt;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (worldMapIcon == null)
					worldMapIcon = GetComponent<WorldMapIcon>();
				return;
			}
#endif
			// instances = instances.Add(this);
		}

		bool interactInput;
		bool previousInteractInput;
		public virtual void DoUpdate ()
		{
			interactInput = InputManager.GetInteractInput(MathfExtensions.NULL_INT);
			if (interactInput && !previousInteractInput && !PauseMenu.Instance.gameObject.activeSelf)
			{
				PauseMenu.Instance.Show ();
				PauseMenu.Instance.SetSection (PauseMenu.Section.WorldMap.GetHashCode());
			}
			previousInteractInput = interactInput;
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
			// instances = instances.Remove(this);
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			if (GameManager.paused)
				return;
			WorldMapIcon worldMapIcon = other.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			playerIsAtObelisk = true;
			GameManager.updatables = GameManager.updatables.Add(this);
			Player.instance.spawnPosition = Player.instance.trs.position;
			if (!playerJustFastTraveled)
			{
				foreach (SkipManager.Skip skip in SkipManager.Instance.skips)
				{
					if (skip.start.name == nameOfLastObeliskPlayerWasAt)
						SkipManager.Instance.RedeemSkip (skip);
				}
			}
			nameOfLastObeliskPlayerWasAt = name;
			if (found)
				SaveAndLoadManager.Instance.SaveToCurrentAccount ();
			else
				OnFound ();
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
			if (PauseMenu.Instance.gameObject.activeSelf)
				return;
			if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
				return;
			playerIsAtObelisk = false;
			playerJustFastTraveled = false;
		}

		public virtual void OnFound ()
		{
			if (ArchivesManager.currentAccountIndex != -1)
				ArchivesManager.CurrentlyPlaying.ObelisksTouched ++;
			foundIndicator.SetActive(true);
			found = true;
			SaveAndLoadManager.Instance.SaveToCurrentAccount ();
		}
	}
}