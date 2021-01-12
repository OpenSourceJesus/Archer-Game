using UnityEngine;
using Extensions;

namespace ArcherGame
{
	[ExecuteInEditMode]
	public class ShooterTrap : Shooter
	{
		EventManager.Event _event;
		public float maxShootDistFromPlayer;
		[HideInInspector]
		public float maxShootDistFromPlayerSqr;
		int framesSinceLoadedSceneAtLastShot;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				maxShootDistFromPlayerSqr = maxShootDistFromPlayer * maxShootDistFromPlayer;
				return;
			}
#endif
			_event = new EventManager.Event();
			_event.time = MathfExtensions.CeilInterval(_event.time, shootTimer.duration);
			_event.onEvent += Shoot;
			EventManager.events.Add(_event);
		}
		
		bool shouldShoot = true;
		public override void Shoot (params object[] args)
		{
			if (shouldShoot)
				base.Shoot (args);
			shouldShoot = ((Vector2) (Player.instance.trs.position - trs.position)).sqrMagnitude <= maxShootDistFromPlayerSqr && GameManager.framesSinceLoadedScene > framesSinceLoadedSceneAtLastShot;
			framesSinceLoadedSceneAtLastShot = GameManager.framesSinceLoadedScene;
			_event.time += shootTimer.duration;
			EventManager.events.Add(_event);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			EventManager.events.Remove(_event);
			_event.onEvent -= Shoot;
		}
	}
}