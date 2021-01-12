namespace ArcherGame
{
	public class UpgradablePerk : Perk
	{
		[SaveAndLoadValue(false)]
		public int timesPurchased;

		public override void Init ()
		{
			for (int i = 0; i < timesPurchased; i ++)
				Apply ();
		}

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
				timesPurchased ++;
			base.Buy ();
		}
	}
}