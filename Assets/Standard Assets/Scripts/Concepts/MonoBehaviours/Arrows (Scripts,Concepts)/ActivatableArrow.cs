using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class ActivatableArrow : Arrow
	{
		protected bool isActivated;
		protected bool arrowActionInput;
		protected bool previousArrowActionInput;

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			isActivated = false;
		}

		public override void DoUpdate ()
		{
			base.DoUpdate ();
			arrowActionInput = InputManager.GetArrowActionInput(owner.playerIndex);
			if (Time.time - Player.timeOfLastShot >= Time.deltaTime * 2 && arrowActionInput && !previousArrowActionInput)
			{
				int indexOfArrowEntry = Player.instance.GetArrowEntryIndex(GetType());
				if (indexOfArrowEntry == Player.instance.currentArrowEntryIndex)
				{
					isActivated = !isActivated;
					if (isActivated)
						Activate ();
					else
						Deactivate ();
				}
			}
			previousArrowActionInput = arrowActionInput;
		}

		public virtual void Activate ()
		{
		}

		public virtual void Deactivate ()
		{
		}
	}
}