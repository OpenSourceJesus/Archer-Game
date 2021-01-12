using UnityEngine;
using Extensions;
using ArcherGame;
using System.Collections;

[RequireComponent(typeof(TrailRenderer))]
public class TravelingBendingLineSegment2D : Spawnable, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public float moveSpeed;
	public float turnDegrees;
	public LayerMask whatICrashInto;
	public TrailRenderer trailRenderer;
	public float lifeDuration;

	void OnEnable ()
	{
		trailRenderer.startColor = ColorExtensions.RandomColor().SetAlpha(trailRenderer.startColor.a);
		trailRenderer.endColor = trailRenderer.startColor;
		trailRenderer.emitting = true;
		GameManager.updatables = GameManager.updatables.Add(this);
		StartCoroutine(DelayDisable ());
	}

	public void DoUpdate ()
	{
		if (Physics2D.Raycast(trs.position, trs.up, moveSpeed * Time.deltaTime, whatICrashInto))
		{
			if (Random.value < 0.5f)
			{
				if (!HandleTurning (Vector3.forward))
				{
					if (!HandleTurning (-Vector3.forward))
					{
						StopCoroutine(DelayDisable ());
						trailRenderer.emitting = false;
						ObjectPool.instance.DelayDespawn (prefabIndex, gameObject, trs, trailRenderer.time);
						GameManager.updatables = GameManager.updatables.Remove(this);
					}
				}
			}
			else if (!HandleTurning (-Vector3.forward))
			{
				if (!HandleTurning (Vector3.forward))
				{
					StopCoroutine(DelayDisable ());
					trailRenderer.emitting = false;
					ObjectPool.instance.DelayDespawn (prefabIndex, gameObject, trs, trailRenderer.time);
					GameManager.updatables = GameManager.updatables.Remove(this);
				}
			}
		}
		trs.position += trs.up * moveSpeed * Time.deltaTime;
	}

	bool HandleTurning (Vector3 rotationAxis)
	{
		trs.RotateAround(trs.position, rotationAxis, turnDegrees);
		for (float angle = turnDegrees; angle < 360; angle += turnDegrees)
		{
			if (Physics2D.Raycast(trs.position, trs.up, moveSpeed * Time.deltaTime, whatICrashInto))
				trs.RotateAround(trs.position, rotationAxis, turnDegrees);
			else
				return true;
		}
		return false;
	}

	void OnDisable ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

	IEnumerator DelayDisable ()
	{
		yield return new WaitForSeconds(lifeDuration);
		trailRenderer.emitting = false;
		ObjectPool.instance.DelayDespawn (prefabIndex, gameObject, trs, trailRenderer.time);
		GameManager.updatables = GameManager.updatables.Remove(this);
	}
}