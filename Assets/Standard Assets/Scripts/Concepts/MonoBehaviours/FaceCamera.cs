using UnityEngine;
using System.Collections;

namespace ArcherGame
{
	[DisallowMultipleComponent]
	//[ExecuteInEditMode]
	public class FaceCamera : MonoBehaviour
	{
		public Transform trs;
		public int xMultiplier;
		public int yMultiplier;
		public int zMultiplier;
		Vector3 toCamera;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				return;
			}
#endif
		}

		public virtual void LateUpdate ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			toCamera = CameraScript.Instance.trs.position - trs.position;
			trs.rotation = Quaternion.LookRotation(transform.forward, new Vector3(toCamera.x * xMultiplier, toCamera.y * yMultiplier, toCamera.z * zMultiplier));
		}
	}
}