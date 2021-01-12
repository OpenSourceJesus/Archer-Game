using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	[RequireComponent(typeof(Rigidbody2D))]
	[DisallowMultipleComponent]
	public class _Rigidbody2D : MonoBehaviour
	{
		public static List<Rigidbody2D> allInstances = new List<Rigidbody2D>();
		public Rigidbody2D rigid;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (rigid == null)
					rigid = GetComponent<Rigidbody2D>();
				return;
			}
#endif
			allInstances.Add(rigid);
		}
		
		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			allInstances.Remove(rigid);
		}
	}
}