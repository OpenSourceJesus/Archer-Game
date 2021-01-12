using System.Collections;
using UnityEngine;
using System;
using Extensions;
using TMPro;
using ArcherGame;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
// using InputDevice = UnityEngine.InputSystem.InputDevice;

namespace DialogAndStory
{
	// [ExecuteInEditMode]
	public class Dialog : MonoBehaviour, IUpdatable
	{
		public bool IsActive
		{
			get
			{
				return gameObject.activeSelf;
			}
			set
			{
				gameObject.SetActive(value);
			}
		}
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Canvas canvas;
		public TMP_Text text;
		[Multiline(25)]
		public string textString;
		public float writeSpeed;
		public int maxCharacters = int.MaxValue;
		public WaitEvent[] waitEvents;
		public SetMaxCharactersEvent[] setMaxCharactersEvents = new SetMaxCharactersEvent[0];
		public RequireInputDeviceEvent[] requireInputDeviceEvents;
		public CustomDialogEvent[] customDialogEvents;
		bool shouldDisplayCurrentChar;
		[HideInInspector]
		public Conversation conversation;
		[HideInInspector]
		public bool isFinished;
		public bool IsFinished
		{
			get
			{
				return isFinished;
			}
			set
			{
				isFinished = value;
			}
		}
		public CustomEvent onStartedEvent;
		public CustomEvent onFinishedEvent;
		public CustomEvent onLeftWhileTalkingEvent;
		public bool autoEnd;
		public bool runWhilePaused;
#if UNITY_EDITOR
		public bool autoSetTextSize;
#endif
		string textStringCopy;
		float writeTimer;
		float writeDelayTime;
		int currentChar;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (autoSetTextSize)
					EditorCoroutineUtility.StartCoroutine(AutoSetTextSize (), this);
				return;
			}
#endif
			// StartCoroutine(AutoSetTextSize ());
			currentChar = 0;
			text.text = "";
			isFinished = false;
			writeTimer = 0;
			writeDelayTime = 0;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
		
		public virtual void DoUpdate ()
		{
			textStringCopy = textString;
			if (runWhilePaused)
				writeTimer += Time.unscaledDeltaTime;
			else
				writeTimer += GameManager.UnscaledDeltaTime;
			if (writeTimer > 1f / writeSpeed + writeDelayTime)
			{
				shouldDisplayCurrentChar = true;
				writeTimer -= (1f / writeSpeed + writeDelayTime);
				writeDelayTime = 0;
				foreach (WaitEvent waitEvent in waitEvents)
				{
					if (textStringCopy.IndexOf(waitEvent.indicator, currentChar) == currentChar)
					{
						writeDelayTime = waitEvent.duration;
						currentChar += waitEvent.indicator.Length;
						shouldDisplayCurrentChar = false;
						break;
					}
				}
				if (writeDelayTime == 0)
				{
					foreach (SetMaxCharactersEvent setMaxCharactersEvent in setMaxCharactersEvents)
					{
						if (textStringCopy.IndexOf(setMaxCharactersEvent.indicator, currentChar) == currentChar)
						{
							maxCharacters = setMaxCharactersEvent.maxCharacters;
							currentChar += setMaxCharactersEvent.indicator.Length;
							shouldDisplayCurrentChar = false;
							break;
						}
					}
					if (shouldDisplayCurrentChar)
					{
						if (textStringCopy.IndexOf(RequireInputDeviceEvent.endIndicator, currentChar) == currentChar)
						{
							currentChar += RequireInputDeviceEvent.endIndicator.Length;
							shouldDisplayCurrentChar = false;
						}
						else
						{
							foreach (RequireInputDeviceEvent requireInputDeviceEvent in requireInputDeviceEvents)
							{
								if (textStringCopy.IndexOf(requireInputDeviceEvent.startIndicator, currentChar) == currentChar)
								{
									shouldDisplayCurrentChar = false;
									if (InputSystem.devices.Contains(requireInputDeviceEvent.inputDevice) == requireInputDeviceEvent.mustHaveInputDevice)
									{
										textStringCopy = textStringCopy.Remove(textStringCopy.IndexOf(RequireInputDeviceEvent.endIndicator, currentChar), RequireInputDeviceEvent.endIndicator.Length);
										currentChar += requireInputDeviceEvent.startIndicator.Length;
									}
									else
									{
										int indexOfEventEnd = textStringCopy.IndexOf(RequireInputDeviceEvent.endIndicator, currentChar) + RequireInputDeviceEvent.endIndicator.Length;
										textStringCopy = textStringCopy.RemoveStartEnd(currentChar, indexOfEventEnd);
										currentChar = indexOfEventEnd;
									}
								}
							}
						}
						foreach (CustomDialogEvent customDialogEvent in customDialogEvents)
						{
							if (textStringCopy.IndexOf(customDialogEvent.indicator, currentChar) == currentChar)
							{
								shouldDisplayCurrentChar = false;
								currentChar += customDialogEvent.indicator.Length;
								customDialogEvent.customEvent.Do ();
							}
						}
					}
				}
				if (shouldDisplayCurrentChar)
				{
					if (currentChar < textStringCopy.Length)
					{
						text.text += textStringCopy[currentChar];
						while (text.text.Length > maxCharacters)
							text.text = text.text.Substring(1);
						currentChar ++;
					}
					else
					{
						isFinished = true;
						onFinishedEvent.Do ();
						if (autoEnd)
							DialogManager.Instance.EndDialog (this);
					}
				}
			}
		}

		IEnumerator AutoSetTextSize ()
		{
#if UNITY_EDITOR
			autoSetTextSize = false;
#endif
			textStringCopy = textString;
			foreach (WaitEvent waitEvent in waitEvents)
				textStringCopy = textStringCopy.Replace(waitEvent.indicator, "");
			foreach (SetMaxCharactersEvent setMaxCharactersEvent in setMaxCharactersEvents)
				textStringCopy = textStringCopy.Replace(setMaxCharactersEvent.indicator, "");
			foreach (RequireInputDeviceEvent requireInputDeviceEvent in requireInputDeviceEvents)
				textStringCopy = textStringCopy.Replace(requireInputDeviceEvent.startIndicator, "");
			textStringCopy = textStringCopy.Replace(RequireInputDeviceEvent.endIndicator, "");
			foreach (CustomDialogEvent customDialogEvent in customDialogEvents)
				textStringCopy = textStringCopy.Replace(customDialogEvent.indicator, "");
			float textSize;
			text.enableAutoSizing = true;
			text.text = textStringCopy;
			yield return new WaitForEndOfFrame();
			textSize = text.fontSize;
			text.enableAutoSizing = false;
			text.fontSize = textSize;
			yield break;
		}

		[Serializable]		
		public class Event
		{
		}

		[Serializable]
		public class WaitEvent : Event
		{
			public string indicator;
			public float duration;
		}
		
		[Serializable]
		public class SetMaxCharactersEvent : Event
		{
			public string indicator;
			public int maxCharacters;
		}
		
		[Serializable]
		public class RequireInputDeviceEvent : Event
		{
			public string startIndicator;
			public const string endIndicator = "{end}";
			public InputDevice inputDevice;
			public bool mustHaveInputDevice;
		}

		[Serializable]
		public class CustomDialogEvent : Event
		{
			public string indicator;
			public CustomEvent customEvent;
		}
	}
}