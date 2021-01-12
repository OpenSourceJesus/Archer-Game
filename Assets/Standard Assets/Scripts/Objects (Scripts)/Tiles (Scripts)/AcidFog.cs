using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class AcidFog : MonoBehaviour
	{
		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			if (other == Player.instance.collider)
				Player.instance.Death ();
			else
			{
				Arrow arrow = other.GetComponent<Arrow>();
				if (arrow != null)
					arrow.gameObject.SetActive(false);
			}
		}
	}
}