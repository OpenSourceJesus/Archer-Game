using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[RequireComponent(typeof(LineRenderer))]
	public class Laser : Hazard, ISpawnable, IUpdatable
	{
		public Transform trs;
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public virtual bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		[MakeConfigurable]
		public float maxLength;
		public LayerMask whatBlocksMe;
		[MakeConfigurable]
		public float activateTime;
		public EdgeCollider2D edgeCollider;
		public LineRenderer line;
		public LineRenderer timeRemainingLine;
		[HideInInspector]
		public RaycastHit2D hit;
		[HideInInspector]
		public RaycastHit2D damaging;
		[HideInInspector]
		public Vector2[] verticies;
		public float multiplyAlphaOnActivate;
		float timeRemaining;
		[HideInInspector]
		public float duration;
		
		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			verticies = new Vector2[2] {Vector2.zero, Vector2.up};
			line.positionCount = 2;
			line.SetPosition(0, verticies[0]);
			line.SetPosition(1, verticies[1]);
			StartCoroutine(Activate ());
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
			edgeCollider.enabled = false;
			Color color = line.endColor.DivideAlpha(multiplyAlphaOnActivate);
			line.startColor = color;
			line.endColor = color;
		}
		
		public virtual IEnumerator Activate ()
		{
			yield return new WaitForSeconds(activateTime);
			timeRemaining = duration;
			Color color = line.endColor.MultiplyAlpha(multiplyAlphaOnActivate);
			line.startColor = color;
			line.endColor = color;
			edgeCollider.points = verticies;
			edgeCollider.enabled = true;
		}
		
		public virtual void DoUpdate ()
		{
			hit = Physics2D.Raycast(trs.position, trs.up, maxLength, whatBlocksMe);
			if (hit.collider != null)
			{
				line.SetPosition(1, Vector2.up * hit.distance);
				timeRemainingLine.SetPosition(1, Vector2.up * hit.distance);
				verticies[1] = Vector2.up * hit.distance;
			}
			else
			{
				line.SetPosition(1, Vector2.up * maxLength);
				timeRemainingLine.SetPosition(1, Vector2.up * maxLength);
				verticies[1] = Vector2.up * maxLength;
			}
			timeRemaining = Mathf.Clamp(timeRemaining - Time.deltaTime, 0, Mathf.Infinity);
			timeRemainingLine.widthMultiplier = line.widthMultiplier * (timeRemaining / duration);
			edgeCollider.points = verticies;
		}
	}
}