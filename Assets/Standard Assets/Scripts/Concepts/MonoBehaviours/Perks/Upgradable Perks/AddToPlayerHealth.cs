namespace ArcherGame
{
	public class AddToPlayerHealth : UpgradablePerk
	{
		public override void Apply ()
		{
			base.Apply ();
			Player.instance.maxHp ++;
			Player.instance.Hp ++;
			Instantiate(Player.instance.lifeIconsParent.GetChild(0).gameObject, Player.instance.lifeIconsParent);
		}
	}
}