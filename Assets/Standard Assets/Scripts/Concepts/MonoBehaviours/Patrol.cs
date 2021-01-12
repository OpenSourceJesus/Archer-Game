using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Patrol : MonoBehaviour, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public PlatformerEntity platformerEntity;
		public Shape2D shape;
		Vector2 destination;
#if UNITY_EDITOR
		public Transform[] shapeCornerPoints = new Transform[0];
#endif

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Vector2[] shapeCorners = new Vector2[shapeCornerPoints.Length];
				for (int i = 0; i < shapeCorners.Length; i ++)
					shapeCorners[i] = shapeCornerPoints[i].position;
				shape = new Shape2D(shapeCorners);
				return;
			}
#endif
			destination = shape.GetRandomPoint();
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void DoUpdate ()
		{
			Rect colliderRect = platformerEntity.ColliderRect;
			platformerEntity.Move(destination - (Vector2) colliderRect.center);
			if (platformerEntity.rigid.velocity.x != 0)
				platformerEntity.trs.localScale = platformerEntity.trs.localScale.SetX(Mathf.Sign(platformerEntity.rigid.velocity.x));
			if (colliderRect.Contains(destination))
				destination = shape.GetRandomPoint();
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}