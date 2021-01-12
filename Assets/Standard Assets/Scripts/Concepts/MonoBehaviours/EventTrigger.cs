using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Extensions;
using UnityEngine.Serialization;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class EventTrigger : MonoBehaviour, IUpdatable
	{
		public UnityEvent onTriggerEnter2D;
		public UnityEvent onTriggerExit2D;
		// public UnityEvent onCollisionEnter2D;
		// public UnityEvent onCollisionExit2D;
		public InputButtonTrigger[] inputButtonTriggers = new InputButtonTrigger[0];
		public InputAxisTrigger[] inputAxisTriggers = new InputAxisTrigger[0];
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public bool runWhilePaused;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (inputButtonTriggers.Length > 0 || inputAxisTriggers.Length > 0)
				GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (inputButtonTriggers.Length > 0 || inputAxisTriggers.Length > 0)
				GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DoUpdate ()
		{
			if (!runWhilePaused && GameManager.paused)
				return;
			foreach (InputButtonTrigger inputButtonTrigger in inputButtonTriggers)
				inputButtonTrigger.Update ();
			foreach (InputAxisTrigger inputAxisTrigger in inputAxisTriggers)
				inputAxisTrigger.Update ();
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			onTriggerEnter2D.Invoke();
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (!gameObject.activeSelf)
				return;
			onTriggerExit2D.Invoke();
		}

		// public virtual void OnCollisionEnter2D (Collision2D coll)
		// {
		// 	onCollisionEnter2D.Invoke();
		// }

		// public virtual void OnCollisionExit2D (Collision2D coll)
		// {
		// 	if (!gameObject.activeSelf)
		// 		return;
		// 	onCollisionExit2D.Invoke();
		// }

		[Serializable]
		public class InputTrigger<T>
		{
			public string inputMemberPath;
			public InputState state;
			public UnityEvent unityEvent;
			[HideInInspector]
			public T value;
			[HideInInspector]
			public T previousValue;

			public virtual void Update ()
			{
				previousValue = value;
				value = InputManager.Instance.GetMember<T>(inputMemberPath);
			}
		}

		[Serializable]
		public class InputButtonTrigger : InputTrigger<bool>
		{
			public override void Update ()
			{
				base.Update ();
				if (state == InputState.Down)
				{
					if (value && !previousValue)
						unityEvent.Invoke();
				}
				else if (state == InputState.Held)
				{
					if (value)
						unityEvent.Invoke();
				}
				else if (!value && previousValue)
					unityEvent.Invoke();
			}
		}

		[Serializable]
		public class InputAxisTrigger : InputTrigger<float>
		{
			public AxisRange axisRange; 

			public override void Update ()
			{
				base.Update ();
				if (state == InputState.Down)
				{
					if (axisRange == AxisRange.Positive)
					{
						if (value > InputManager.Settings.defaultDeadzoneMin && previousValue <= InputManager.Settings.defaultDeadzoneMin)
							unityEvent.Invoke();
					}
					else if (axisRange == AxisRange.Negative)
					{
						if (value < -InputManager.Settings.defaultDeadzoneMin && previousValue >= -InputManager.Settings.defaultDeadzoneMin)
							unityEvent.Invoke();
					}
					else if (Mathf.Abs(value) > InputManager.Settings.defaultDeadzoneMin && Mathf.Abs(previousValue) <= InputManager.Settings.defaultDeadzoneMin)
						unityEvent.Invoke();
				}
				else if (state == InputState.Held)
				{
					if (axisRange == AxisRange.Positive)
					{
						if (value > InputManager.Settings.defaultDeadzoneMin)
							unityEvent.Invoke();
					}
					else if (axisRange == AxisRange.Negative)
					{
						if (value < -InputManager.Settings.defaultDeadzoneMin)
							unityEvent.Invoke();
					}
					else if (Mathf.Abs(value) > InputManager.Settings.defaultDeadzoneMin)
						unityEvent.Invoke();
				}
				else
				{
					if (axisRange == AxisRange.Positive)
					{
						if (value <= InputManager.Settings.defaultDeadzoneMin && previousValue > InputManager.Settings.defaultDeadzoneMin)
							unityEvent.Invoke();
					}
					else if (axisRange == AxisRange.Negative)
					{
						if (value >= -InputManager.Settings.defaultDeadzoneMin && previousValue < -InputManager.Settings.defaultDeadzoneMin)
							unityEvent.Invoke();
					}
					else if (Mathf.Abs(value) <= InputManager.Settings.defaultDeadzoneMin && Mathf.Abs(previousValue) > InputManager.Settings.defaultDeadzoneMin)
						unityEvent.Invoke();
				}
			}

			public enum AxisRange
			{
				Positive,
				Negative,
				Full
			}
		}

		public enum InputState
		{
			Down,
			Held,
			Up
		}
	}
}