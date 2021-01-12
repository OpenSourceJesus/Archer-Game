using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	[RequireComponent(typeof(TMP_Text))]
	[DisallowMultipleComponent]
	public class _Text : MonoBehaviour
	{
		[HideInInspector]
		public string keyboardText;
		public bool useSeperateTextForGamepad;
		[Multiline]
		public string gamepadText;
		public TMP_Text text;
		public static _Text[] instances = new _Text[0];

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (text == null)
					text = GetComponent<TMP_Text>();
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
		}

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			UpdateText ();
			instances = instances.Add(this);
		}
		
		public virtual void UpdateText ()
		{
			if (useSeperateTextForGamepad)
			{
				if (InputManager.UsingGamepad)
					text.text = gamepadText;
				else
					text.text = keyboardText;
			}
			else
				text.text = keyboardText;
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			instances = instances.Remove(this);
		}

#if UNITY_EDITOR
		public virtual void DoEditorUpdate ()
		{
			keyboardText = text.text;
		}
#endif
	}
}
