using UnityEngine;
using Extensions;
using ArcherGame;

public class RotateTransformRandomly : MonoBehaviour
{
	public Timer rotateTimer;
	public Transform trs;
	
	public virtual void Start ()
	{
		rotateTimer.onFinished += Rotate;
	}

	public virtual void OnEnable ()
	{
		rotateTimer.Start ();
	}

	public virtual void OnDisable ()
	{
		rotateTimer.Reset ();
	}

	public virtual void OnDestroy ()
	{
		rotateTimer.onFinished -= Rotate;
	}

	public virtual void Rotate (params object[] args)
	{
		trs.eulerAngles = Vector3.forward * Random.value * 360;
	}
}