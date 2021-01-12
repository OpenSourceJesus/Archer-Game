using System.Collections.Generic;
using DialogAndStory;
using System;

namespace ArcherGame
{
	public class ArrowCollectible : Collectible
	{
		public static Dictionary<Type, Conversation> tutorialConversationsDict = new Dictionary<Type, Conversation>();
		public Arrow arrowType;
		public Conversation tutorialConversation;

		public override void Awake ()
		{
			base.Awake ();
			if (!tutorialConversationsDict.ContainsKey(arrowType.GetType()))
				tutorialConversationsDict.Add(arrowType.GetType(), tutorialConversation);
		}

		public override void OnCollected ()
		{
			base.OnCollected ();
			Player.ArrowEntry arrowEntry = Player.instance.GetArrowEntry(arrowType.GetType());
			arrowEntry.isOwned = true;
			arrowEntry.menuOptionToSwitchToMe.SetActive(true);
			if (tutorialConversation != null)
				DialogManager.Instance.StartConversation (tutorialConversation);
		}
	}
}