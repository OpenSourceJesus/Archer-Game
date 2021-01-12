using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArcherGame;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DialogAndStory
{
	//[ExecuteInEditMode]
	public class Conversation : MonoBehaviour
	{
		public Transform trs;
		public Dialog[] dialogs = new Dialog[0];
		public Coroutine updateRoutine;
		[HideInInspector]
		public Dialog lastStartedDialog;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
		}
		
		public virtual IEnumerator UpdateRoutine ()
		{
			foreach (Dialog dialog in dialogs)
			{
				lastStartedDialog = dialog;
				DialogManager.Instance.StartDialog (dialog);
				yield return new WaitUntil(() => (!dialog.IsActive));
				DialogManager.instance.EndDialog (dialog);
			}
			yield break;
		}
		
		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			EditorApplication.update -= DoEditorUpdate;
			if (!Application.isPlaying)
				return;
#endif
			if (updateRoutine != null)
			{
				StopCoroutine (updateRoutine);
				updateRoutine = null;
				foreach (Dialog dialog in dialogs)
					dialog.gameObject.SetActive(false);
			}
		}
		
#if UNITY_EDITOR
		public virtual void DoEditorUpdate ()
		{
			if (dialogs.Length == 0)
				dialogs = GetComponentsInChildren<Dialog>();
			foreach (Dialog dialog in dialogs)
				dialog.conversation = this;
		}
#endif
	}
}