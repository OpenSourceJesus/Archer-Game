using UnityEngine;
using System.Collections;
using Extensions;

namespace ArcherGame
{
	public class FreezeObject : MonoBehaviour, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Transform trs;
		public bool freezePosition;
		public bool freezeRelativePosition;
		public bool freezeRotation;
		public bool freezeScale;
		Vector3 initRota;
		Vector3 initPos;
		Vector3 initRelativePos;
		Vector3 initWorldScale;

		public virtual void Awake ()
		{
			initRota = trs.eulerAngles;
			initPos = trs.position;
			initRelativePos = trs.position - trs.parent.position;
			initWorldScale = trs.lossyScale;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDestroy ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DoUpdate ()
		{
			if (freezePosition)
				trs.position = initPos;
			else if (freezeRelativePosition)
				trs.position = trs.parent.position + initRelativePos;
			if (freezeRotation)
				trs.eulerAngles = initRota;
			if (freezeScale)
				trs.SetWorldScale (initWorldScale);
		}
	}
}