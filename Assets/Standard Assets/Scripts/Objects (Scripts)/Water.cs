using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Water : SingletonMonoBehaviour<Water>, ICopyable
	{
		Collider2D[] waterRectColliders = new Collider2D[0];
		[HideInInspector]
		public Rect[] waterRects = new Rect[0];
		public float addToLinearDrag;
		public float addToAngularDrag;
		public float subtractFromGravityScale;
		public new Collider2D collider;

#if UNITY_EDITOR
		public virtual void Start ()
		{
			if (!Application.isPlaying)
			{
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
		}

		public virtual void DoEditorUpdate ()
		{
			waterRectColliders = GetComponentsInChildren<Collider2D>().Remove(GetComponent<Collider2D>());
			waterRects = new Rect[waterRectColliders.Length];
			for (int i = 0; i < waterRectColliders.Length; i ++)
				waterRects[i] = waterRectColliders[i].GetRect();
		}

		public virtual void OnDestroy ()
		{
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}
#endif

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			WorldMapIcon worldMapIcon = other.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			PlatformerEntity platformerEntity = other.GetComponent<PlatformerEntity>();
			if (platformerEntity != null)
			{
				// Debug.Log("Enter");
				if (platformerEntity.canSwim && !platformerEntity.isSwimming)
					platformerEntity.StartSwimming ();
			}
			else
			{
				Arrow arrow = other.GetComponent<Arrow>();
				if (GameManager.gameModifierDict["Unslowable Pull Arrows"].isActive && (arrow as PullArrow) != null)
					return;
				arrow.rigid.drag += addToLinearDrag;
				arrow.rigid.angularDrag += addToAngularDrag;
				arrow.rigid.gravityScale -= subtractFromGravityScale;
			}
		}
		
		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
				return;
			PlatformerEntity platformerEntity = other.GetComponent<PlatformerEntity>();
			if (platformerEntity != null)
			{
				// Debug.Log("Exit");
				if (platformerEntity.canSwim && !other.IsTouchingLayers(LayerMask.GetMask("Water")))
					platformerEntity.StopSwimming ();
			}
			else
			{
				Arrow arrow = other.GetComponent<Arrow>();
				if (GameManager.gameModifierDict["Unslowable Pull Arrows"].isActive && (arrow as PullArrow) != null)
					return;
				arrow.rigid.drag -= addToLinearDrag;
				arrow.rigid.angularDrag -= addToAngularDrag;
				arrow.rigid.gravityScale += subtractFromGravityScale;
			}
		}

		public virtual void Copy (object copy)
		{
			Water water = copy as Water;
			addToLinearDrag = water.addToLinearDrag;
			addToAngularDrag = water.addToAngularDrag;
			subtractFromGravityScale = water.subtractFromGravityScale;
			waterRects = water.waterRects;
			collider = GetComponent<Collider2D>();
			collider.isTrigger = water.collider.isTrigger;
		}
	}
}