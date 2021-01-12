using UnityEngine;
using Extensions;
using ArcherGame;
using FastMember;
using System;

public class DisplayMembers : MonoBehaviour, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return false;
		}
	}
	public MonoBehaviourDisplayEntry[] monoBehaviourDisplayEntries = new MonoBehaviourDisplayEntry[0];

	void OnEnable ()
	{
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	public void DoUpdate ()
	{
		for (int i = 0; i < monoBehaviourDisplayEntries.Length; i ++)
		{
			MonoBehaviourDisplayEntry monoBehaviourDisplayEntry = monoBehaviourDisplayEntries[i];
			for (int i2 = 0; i2 < monoBehaviourDisplayEntry.memberPaths.Length; i2 ++)
			{
				string memberPath = monoBehaviourDisplayEntry.memberPaths[i2];
				print(monoBehaviourDisplayEntry.obj.GetMember<object>(memberPath));
			}
		}
	}

	void OnDisable ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

	[Serializable]
	public class DisplayEntry<T>
	{
		public T obj;
		public string[] memberPaths;
	}

	[Serializable]
	public class MonoBehaviourDisplayEntry : DisplayEntry<MonoBehaviour>
	{
	}
}