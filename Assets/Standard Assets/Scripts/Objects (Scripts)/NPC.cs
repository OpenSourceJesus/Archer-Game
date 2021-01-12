using UnityEngine;
using DialogAndStory;
using System.Collections;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class NPC : PlatformerEntity, ISaveableAndLoadable
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
		[SaveAndLoadValue(false)]
		public bool hasApproached;
		public Conversation conversationOnFirstApproach;
		public Conversation conversationAfterFirstApproach;
		public GameObject talkToMeIndicatorGo;
		Conversation currentConversation;
		Coroutine waitForInteractRoutine;

		public override void Start ()
		{
			// base.Start ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
		}

		void OnTriggerEnter2D (Collider2D other)
		{
			if (hasApproached)
			{
				talkToMeIndicatorGo.SetActive(true);
				waitForInteractRoutine = StartCoroutine(WaitForInteract ());
			}
			else
			{
				if (conversationOnFirstApproach != null)
				{
					currentConversation = conversationOnFirstApproach;
					talkToMeIndicatorGo.SetActive(false);
					DialogManager.Instance.StartConversation (conversationOnFirstApproach);
				}
				else
				{
					talkToMeIndicatorGo.SetActive(true);
					waitForInteractRoutine = StartCoroutine(WaitForInteract ());
				}
				hasApproached = true;
			}
		}

		void OnTriggerStay2D (Collider2D other)
		{
			if (currentConversation == null && waitForInteractRoutine == null)
			{
				talkToMeIndicatorGo.SetActive(true);
				waitForInteractRoutine = StartCoroutine(WaitForInteract ());
			}
		}

		void OnTriggerExit2D (Collider2D other)
		{
			talkToMeIndicatorGo.SetActive(false);
			if (currentConversation != null && currentConversation.updateRoutine != null)
				DialogManager.Instance.EndConversation (currentConversation);
			if (waitForInteractRoutine != null)
			{
				StopCoroutine(waitForInteractRoutine);
				waitForInteractRoutine = null;
			}
		}

		IEnumerator WaitForInteract ()
		{
			bool previousInteractInput = false;
			do
			{
				bool interactInput = InputManager.GetInteractInput(MathfExtensions.NULL_INT);
				if (interactInput && !previousInteractInput)
				{
					talkToMeIndicatorGo.SetActive(false);
					currentConversation = conversationAfterFirstApproach;
					DialogManager.Instance.StartConversation (conversationAfterFirstApproach);
					waitForInteractRoutine = null;
					yield break;
				}
				previousInteractInput = interactInput;
				yield return new WaitForEndOfFrame();
			} while (true);
		}
	}
}