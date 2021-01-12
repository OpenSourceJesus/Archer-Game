using UnityEngine;
using System.Collections;

namespace ArcherGame
{
	public class GadamiteSwarm : MonoBehaviour
	{
		void OnTriggerEnter2D (Collider2D other)
		{
			if (other.gameObject != Player.instance.gameObject)
			{
				Destroy(other.gameObject);
				return;
			}
			StopAllCoroutines();
			Player.instance.animator.Play("Cover Quiver", 2);
			StartCoroutine(DisableShootingRoutine ());
		}

		void OnTriggerExit2D (Collider2D other)
		{
			if (other.gameObject != Player.instance.gameObject)
				return;
			StopAllCoroutines();
			Player.instance.animator.Play("Uncover Quiver", 2);
			StartCoroutine(EnableShootingRoutine ());
		}

		IEnumerator DisableShootingRoutine ()
		{
			yield return new WaitUntil(() => (!Player.instance.animator.GetCurrentAnimatorStateInfo(2).IsName("Cover Quiver")));
			Player.instance.canShoot = false;
		}

		IEnumerator EnableShootingRoutine ()
		{
			yield return new WaitUntil(() => (!Player.instance.animator.GetCurrentAnimatorStateInfo(2).IsName("Uncover Quiver")));
			Player.instance.canShoot = true;
		}
	}
}