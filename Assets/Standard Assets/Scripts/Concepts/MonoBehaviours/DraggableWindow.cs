using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using Extensions;

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class DraggableWindow : MonoBehaviour, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public RectTransform rectTrs;
		public RectTransform[] rectTrsOfAreasICantDrag = new RectTransform[0];
		Vector2 previousMousePosition;
		public float normalizedScreenBorder;
		[HideInInspector]
		public Rect screenRectWithoutBorder;
		bool isGrabbed;
		Vector2 previousRectTrsPosition;
		bool leftClickInput;
		bool previousLeftClickInput;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (rectTrs != null)
					rectTrs = GetComponent<RectTransform>();
				return;
			}
#endif
			screenRectWithoutBorder = new Rect();
			screenRectWithoutBorder.size = new Vector2(Screen.width, Screen.height) - (new Vector2(Screen.width, Screen.height) * normalizedScreenBorder);
			screenRectWithoutBorder.center = new Vector2(Screen.width, Screen.height) / 2;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DoUpdate ()
		{
			if (this == null)
				return;
			leftClickInput = InputManager.GetLeftClickInput(MathfExtensions.NULL_INT);
			if (leftClickInput && !previousLeftClickInput && rectTrs.GetWorldRect().Contains(InputManager.GetMousePosition(MathfExtensions.NULL_INT)))
			{
				foreach (RectTransform rectTrsOfAreaICantDrag in rectTrsOfAreasICantDrag)
				{
					if (rectTrsOfAreaICantDrag.GetWorldRect().Contains(InputManager.GetMousePosition(MathfExtensions.NULL_INT)))
						return;
				}
				isGrabbed = true;
			}
			else if (isGrabbed && leftClickInput)
			{
				rectTrs.position += (Vector3) ((Vector2) InputManager.GetMousePosition(MathfExtensions.NULL_INT) - previousMousePosition);
				if (!screenRectWithoutBorder.IsEncapsulating(rectTrs.GetWorldRect(), true))
					rectTrs.position = previousRectTrsPosition;
			}
			else if (!leftClickInput && previousLeftClickInput)
				isGrabbed = false;
			previousMousePosition = InputManager.GetMousePosition(MathfExtensions.NULL_INT);
			previousRectTrsPosition = rectTrs.position;
			previousLeftClickInput = leftClickInput;
		}
	}
}