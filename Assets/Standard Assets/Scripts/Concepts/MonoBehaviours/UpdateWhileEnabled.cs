using UnityEngine;
using ArcherGame;
using Extensions;

public class UpdateWhileEnabled : MonoBehaviour, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}

	public virtual void OnEnable ()
	{
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	public virtual void DoUpdate ()
	{
	}

	public virtual void OnDisable ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

	public virtual void OnDestroy ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}
}