#if UNITY_EDITOR
using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class AddMoney : MonoBehaviour
	{
		public bool update;
		public int amount;

		public virtual void Update ()
		{
			if (!update)
				return;
			update = false;
			Player.instance.AddMoney (amount);
			Player.instance.DisplayMoney ();
			DestroyImmediate(this);
		}
	}
}
#endif