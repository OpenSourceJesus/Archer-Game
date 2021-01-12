using UnityEngine;
using Extensions;
using TMPro;
using UnityEngine.UI;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class SettingsMenu : MonoBehaviour, ISaveableAndLoadable
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
		public GameObject go;
		public Transform trs;
		[SaveAndLoadValue(false)]
		public bool mute;
		[SaveAndLoadValue(false)]
		public float volume;
		[SaveAndLoadValue(false)]
		public int vSyncCount;
		[SaveAndLoadValue(false)]
		public int antiAliasingValue;
		[SaveAndLoadValue(false)]
		public int qualityLevel;
		[SaveAndLoadValue(false)]
		public bool fullscreen;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (go == null)
					go = GetComponent<GameObject>();
				if (trs == null)
					trs = GetComponent<Transform>();
				return;
			}
#endif
		}

		public virtual void Init ()
		{
			
		}

		public virtual void SetMute (bool mute)
		{
			AudioListener.pause = mute;
		}

		public virtual void SetVolume (float volume)
		{
			AudioListener.volume = volume;
		}

		public virtual void SetVSyncCount (int vSyncCount)
		{
			QualitySettings.vSyncCount = vSyncCount;
		}

		public virtual void SetAntiAliasingValue (int antiAliasingValue)
		{
			QualitySettings.antiAliasing = antiAliasingValue;
		}

		public virtual void SetQualityLevel (int index)
		{
			QualitySettings.SetQualityLevel(index);
		}

		public virtual void SetFullscreen (bool fullscreen)
		{
			Screen.fullScreen = fullscreen;
		}
	}
}