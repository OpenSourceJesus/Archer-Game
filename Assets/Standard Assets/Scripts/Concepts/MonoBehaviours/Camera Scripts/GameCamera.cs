#if UNITY_EDITOR
using UnityEngine;
#endif
using Extensions;

namespace ArcherGame
{
	public class GameCamera : CameraScript
	{
		public override void Awake ()
		{
			Player.instance = FindObjectOfType<Player>();
			base.Awake ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			trs.SetParent(null);
		}

		public override void HandlePosition ()
		{
			trs.position = Player.instance.trs.position.SetZ(trs.position.z);
			base.HandlePosition ();
		}
	}
}