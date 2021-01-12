using UnityEngine;
using UnityEngine.UI;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class PerksMenu : SingletonMonoBehaviour<PerksMenu>
	{
		public Canvas canvas;
		public int addToPerkCosts;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
			// gameObject.SetActive(false);
			canvas.enabled = false;
		}

		public virtual void ShowSkipsScreen ()
		{
			WorldMap.Instance.Open ();
		}

		public virtual void HideSkipsScreen ()
		{
			WorldMap.Instance.Close ();
		}
	}
}