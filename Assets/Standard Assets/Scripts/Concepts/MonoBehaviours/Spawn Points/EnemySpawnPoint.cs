namespace ArcherGame
{
	public class EnemySpawnPoint : SpawnPoint
	{
		public Enemy[] enemiesThatUseMe = new Enemy[0];
		public float minSpawnRangeFromPlayer;
	}
}