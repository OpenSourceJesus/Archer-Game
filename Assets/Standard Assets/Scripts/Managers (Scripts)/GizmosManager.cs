using UnityEngine;
using System;
using System.Collections.Generic;

public class GizmosManager : MonoBehaviour
{
	public static List<GizmosEntry> gizmosEntries = new List<GizmosEntry>();

	public virtual void OnDrawGizmos ()
	{
		GizmosEntry gizmosEntry;
		for (int i = 0; i < gizmosEntries.Count; i ++)
		{
			gizmosEntry = gizmosEntries[i];
			if (gizmosEntry.setColor)
				Gizmos.color = gizmosEntry.color;
			gizmosEntry.onDrawGizmos (gizmosEntry.args);
			if (gizmosEntry.remove)
				gizmosEntries.RemoveAt(i);
		}
	}

	public class GizmosEntry
	{
		public Action<object[]> onDrawGizmos;
		public object[] args;
		public bool setColor;
		public Color color;
		public bool remove;
	}
}