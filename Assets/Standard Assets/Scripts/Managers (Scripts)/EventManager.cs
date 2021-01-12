using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class EventManager : SingletonMonoBehaviour<EventManager>, IUpdatable
	{
		public static List<Event> events = new List<Event>();
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		Event _event;
		public virtual void DoUpdate ()
		{
			for (int i = 0; i < events.Count; i ++)
			{
				_event = events[i];
				if (Time.timeSinceLevelLoad >= _event.time)
				{
					_event.onEvent (_event.args);
					events.RemoveAt(i);
					i --;
				}
			}
		}

		public class Event
		{
			public Action<object[]> onEvent;
			public object[] args;
			public float time;

			public Event ()
			{
			}

			public Event (Action<object[]> onEvent, object[] args, float time)
			{
				this.onEvent = onEvent;
				this.args = args;
				this.time = time;
			}
		}
	}
}