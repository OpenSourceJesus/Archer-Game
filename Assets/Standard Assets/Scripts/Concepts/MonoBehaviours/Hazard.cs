using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class Hazard : MonoBehaviour, IConfigurable
	{
		public virtual string Name
		{
			get
			{
				return name;
			}
		}
		public virtual string Category
		{
			get
			{
				return "Hazards";
			}
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			OnTriggerStay2D (other);
		}

		public virtual void OnTriggerStay2D (Collider2D other)
		{
			if (enabled)
			{
				IDestructable destructable = other.GetComponent<IDestructable>();
				if (destructable != null)
					destructable.TakeDamage (0);
			}
		}
	}
}