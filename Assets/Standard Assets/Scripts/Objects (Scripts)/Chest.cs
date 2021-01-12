using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Chest : Collectible
	{
		public int money;
		
		public override void OnCollected ()
		{
			base.OnCollected ();
			if (!collectedAndSaved)
			{
				Player.addToMoneyOnSave += money;
				Player.instance.DisplayMoney ();
			}
		}
	}
}