using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace ArcherGame
{
	public class Boss : MultipartEnemy, ISaveableAndLoadable
	{
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
		// [SaveAndLoadValue(false)]
		// public new bool isDead;
		public string description;
		public GameObject infoParentGo;
		public TMP_Text descriptionText;

		public override void Start ()
		{
			base.Start ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (nextPart != null)
				return;
			if (isDead)
				Destroy(parts[0].gameObject);
			else
				StartBattle ();
		}

		public override void Death ()
		{
			base.Death ();
			if (nextPart != null)
				nextPart.gameObject.SetActive(true);
			else
				isDead = true;
		}

		public virtual void StartBattle ()
		{
			descriptionText.text = description;
			for (int i = 0; i < parts.Length - 1; i ++)
				parts[i].GetComponent<Boss>().healthbarTrs = Instantiate(healthbarTrs, healthbarTrs.parent);
			infoParentGo.SetActive(true);
		}

		public virtual void EndBattle ()
		{
			infoParentGo.SetActive(false);
		}
	}
}