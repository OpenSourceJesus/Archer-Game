using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;

namespace ArcherGame
{
	public class MultipartEnemy : Enemy
	{
		public Transform[] parts = new Transform[0];
		public Transform nextPart;

		public override void Start ()
		{
			base.Start ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (parts.Length == 0)
				{
					while (trs.GetChild(0).GetComponent<MultipartEnemy>() != null)
						parts = parts.Add(trs.GetChild(0));
					if (nextPart == null)
						nextPart = trs.GetChild(0);
				}
				return;
			}
#endif
			if (nextPart != null)
			{
				foreach (Transform part in parts)
				{
					part.gameObject.SetActive(false);
					part.SetParent(null);
				}
				return;
			}
		}
	}
}