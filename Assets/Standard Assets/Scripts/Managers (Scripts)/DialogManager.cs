using UnityEngine;

namespace DialogAndStory
{
	public class DialogManager : SingletonMonoBehaviour<DialogManager>
	{
		public void StartDialog (Dialog dialog)
		{
			dialog.onStartedEvent.Do ();
			dialog.IsActive = true;
		}
		
		public void EndDialog (Dialog dialog)
		{
			if (dialog == null)
				return;
			if (!dialog.isFinished)
				dialog.onLeftWhileTalkingEvent.Do ();
			dialog.IsActive = false;
		}
		
		public void StartConversation (Conversation conversation)
		{
			conversation.updateRoutine = conversation.StartCoroutine(conversation.UpdateRoutine ());
		}

		public void EndConversation (Conversation conversation)
		{
			EndDialog (conversation.lastStartedDialog);
			// for (int i = 0; i < conversation.dialogs.Length; i ++)
			// {
			// 	Dialog dialog = conversation.dialogs[i];
			// 	EndDialog (dialog);
			// }
			if (conversation.updateRoutine != null)
				conversation.StopCoroutine(conversation.updateRoutine);
			conversation.updateRoutine = null;
		}
	}
}