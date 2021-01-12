#if UNITY_EDITOR
using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class SetMoney : MonoBehaviour
	{
		public bool update;
		public int amount;

		public virtual void Update ()
		{
			if (!update)
				return;
			update = false;
			Player.instance.AddMoney (amount - ArchivesManager.CurrentlyPlaying.CurrentMoney);
			Player.instance.DisplayMoney ();
			DestroyImmediate(this);
		}
	}
}
#endif