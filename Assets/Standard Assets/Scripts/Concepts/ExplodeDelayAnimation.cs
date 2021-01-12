using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{    
	public class ExplodeDelayAnimation : StateMachineBehaviour
	{
		public override void OnStateExit (Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
		{
			animator.GetComponent<Bomb>().Explode ();
		}
	}
}