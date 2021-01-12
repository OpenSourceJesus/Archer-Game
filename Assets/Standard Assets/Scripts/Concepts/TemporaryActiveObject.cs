using UnityEngine;
using System.Collections;
using System;
using Extensions;

namespace ArcherGame
{
	[Serializable]
	public class TemporaryActiveObject
	{
		public GameObject obj;
		public float duration;
		public bool realtime;
		public static TemporaryActiveObject[] activeInstances = new TemporaryActiveObject[0];

		public virtual void Do ()
		{
			GameManager.Instance.StartCoroutine(DoRoutine ());
		}

		public virtual void Stop ()
		{
			GameManager.Instance.StopCoroutine(DoRoutine ());
		}
		
		public virtual IEnumerator DoRoutine ()
		{
			Activate ();
			if (realtime)
				yield return new WaitForSecondsRealtime(duration);
			else
				yield return new WaitForSeconds(duration);
			Deactivate ();
		}

		public virtual void Activate ()
		{
			if (activeInstances.Contains(this))
				return;
			if (obj != null)
				obj.SetActive(true);
			activeInstances = activeInstances.Add(this);
		}

		public virtual void Deactivate ()
		{
			if (obj != null)
				obj.SetActive(false);
			activeInstances = activeInstances.Remove(this);
		}
	}
}