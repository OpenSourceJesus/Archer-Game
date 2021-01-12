namespace ArcherGame
{
	public class AddToSkips : UpgradablePerk
	{
		public override void Buy ()
		{
			if (!Obelisk.playerIsAtObelisk)
			{
				if (ArchivesManager.CurrentlyPlaying.CurrentMoney >= cost)
					GameManager.Instance.notificationText.text.text = "You must be at an obelisk to buy perks!";
				else
					GameManager.Instance.notificationText.text.text = "You must be at an obelisk to buy perks! You don't have enough money anyway!";
				GameManager.Instance.notificationText.Do ();
				return;
			}
			if (ArchivesManager.CurrentlyPlaying.CurrentMoney >= cost)
				SkipManager.skipPoints ++;
			base.Buy ();
		}
	}
}