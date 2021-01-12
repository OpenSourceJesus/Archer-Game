using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class Collectible : MonoBehaviour, ISaveableAndLoadable
	{
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		public static Collectible[] instances = new Collectible[0];
		[SaveAndLoadValue(false)]
		public bool collectedAndSaved;
		public bool collected;

		public virtual void Awake ()
		{
			instances = instances.Add(this);
		}

		public virtual void OnDestroy ()
		{
			instances = instances.Remove(this);
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			OnCollected ();
			collected = true;
		}

		public virtual void OnCollected ()
		{
			gameObject.SetActive(false);
		}
	}
}