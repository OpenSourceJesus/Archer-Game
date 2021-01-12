namespace ArcherGame
{
	public class AddToArrowDamage : UpgradablePerk
	{
		public float addToDamageMultiplier;

		public override void Apply ()
		{
			base.Apply ();
			for (int i = 0; i < Player.instance.arrowEntries.Length; i ++)
			{
				Player.ArrowEntry arrowEntry = Player.instance.arrowEntries[i];
				arrowEntry.arrowPrefab.damageMultiplier += addToDamageMultiplier;
				for (int i2 = 0; i2 < arrowEntry.arrows.Length; i2 ++)
				{
					Arrow arrow = arrowEntry.arrows[i2];
					arrow.damageMultiplier += addToDamageMultiplier;
				}
				Player.instance.arrowEntries[i] = arrowEntry;
			}
		}
	}
}