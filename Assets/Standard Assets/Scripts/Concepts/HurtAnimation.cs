using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{    
	public class HurtAnimation : StateMachineBehaviour
	{
		public override void OnStateExit (Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
		{
			Player.instance.isHurting = false;
		}
	}
}