using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class _Transform : MonoBehaviour
	{
		public Transform trs;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				trs = GetComponent<Transform>();
				return;
			}
#endif
		}
		
		public virtual void SetWorldScaleX (float scaleX)
		{
			trs.localScale = trs.localScale.SetX(trs.worldToLocalMatrix.MultiplyVector(trs.localScale * scaleX).x);
		}
		
		public virtual void SetWorldScaleY (float scaleY)
		{
			trs.localScale = trs.localScale.SetY(trs.worldToLocalMatrix.MultiplyVector(trs.localScale * scaleY).y);
		}
		
		public virtual void SetWorldScaleZ (float scaleZ)
		{
			trs.localScale = trs.localScale.SetZ(trs.worldToLocalMatrix.MultiplyVector(trs.localScale * scaleZ).z);
		}
		
		public virtual void SetWorldScale (Vector3 scale)
		{
			trs.localScale = trs.worldToLocalMatrix.MultiplyVector(trs.localScale.Multiply(scale));
		}
		
		public virtual void TeleportTo (Transform trs)
		{
			this.trs.position = trs.position;
		}
	}
}