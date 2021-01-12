using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;
using UnityEngine.Tilemaps;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class SpiderWeb : SingletonMonoBehaviour<SpiderWeb>, IUpdatable, ICopyable
	{
		public float addLinearDrag;
		public float addAngularDrag;
		public static List<StuckEntry> stuckEntries = new List<StuckEntry>();
		public virtual bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public new Collider2D collider;

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			WorldMapIcon worldMapIcon = other.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			PlatformerEntity platformerEntity = other.GetComponent<PlatformerEntity>();
			StuckEntry stuckEntry = null;
			if (platformerEntity != null)
				stuckEntry = new StuckEntry(platformerEntity);
			else
			{
				Arrow arrow = other.GetComponent<Arrow>();
				if (arrow != null)
				{
					if (GameManager.gameModifierDict["Unslowable Pull Arrows"].isActive && (arrow as PullArrow) != null)
						return;
					stuckEntry = new StuckEntry(arrow);
				}
				else
					return;
			}
			stuckEntries.Add(stuckEntry);
			if (stuckEntries.Count == 1)
				GameManager.updatables = GameManager.updatables.Add(this);
		}

		StuckEntry stuckEntry;
		public virtual void DoUpdate ()
		{
			for (int i = 0; i < stuckEntries.Count; i ++)
			{
				stuckEntry = stuckEntries[i];
				if (stuckEntry.rigid != null)
					stuckEntry.Update ();
				else
				{
					stuckEntries.RemoveAt(i);
					i --;
					if (stuckEntries.Count == 0)
						GameManager.updatables = GameManager.updatables.Remove(this);
				}
			}
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			stuckEntries.Clear();
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
		
		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
				return;
			Rigidbody2D rigid = other.GetComponent<Rigidbody2D>();
			StuckEntry stuckEntry = null;
			foreach (StuckEntry _stuckEntry in stuckEntries)
			{
				if (_stuckEntry.rigid == rigid)
				{
					stuckEntry = _stuckEntry;
					break;
				}
			}
			if (stuckEntry == null)
				return;
			stuckEntries.Remove(stuckEntry);
			if (stuckEntries.Count == 0)
				GameManager.updatables = GameManager.updatables.Remove(this);
			stuckEntry.End ();
		}

		public class StuckEntry
		{
			public Rigidbody2D rigid;
			public Arrow arrow;
			public PlatformerEntity platformerEntity;
			// float originalLinearDrag;

			public StuckEntry (PlatformerEntity platformerEntity) : this (platformerEntity.GetComponent<Rigidbody2D>())
			{
				this.platformerEntity = platformerEntity;
			}

			public StuckEntry (Arrow arrow) : this (arrow.GetComponent<Rigidbody2D>())
			{
				this.arrow = arrow;
			}

			public StuckEntry (Rigidbody2D rigid)
			{
				this.rigid = rigid;
				// originalLinearDrag = rigid.drag;
				rigid.drag += SpiderWeb.Instance.addLinearDrag;
				rigid.angularDrag += SpiderWeb.Instance.addAngularDrag;
			}
			
			public virtual void Update ()
			{
				if (platformerEntity != null)
				{
					if (platformerEntity.velocityEffectors_Vector2Dict["Pull Arrow"].effect.magnitude > 0)
						rigid.drag = 0;
					else
						rigid.drag = SpiderWeb.Instance.addLinearDrag;
						// rigid.drag = originalLinearDrag + SpiderWeb.Instance.addLinearDrag;
				}
			}

			public virtual void End ()
			{
				// rigid.drag = originalLinearDrag;
				rigid.drag = 0;
				rigid.angularDrag -= SpiderWeb.Instance.addAngularDrag;
			}
		}

		public virtual void Copy (object copy)
		{
			SpiderWeb spiderWeb = copy as SpiderWeb;
			addLinearDrag = spiderWeb.addLinearDrag;
			addAngularDrag = spiderWeb.addAngularDrag;
			collider = GetComponent<Collider2D>();
			collider.isTrigger = spiderWeb.collider.isTrigger;
		}
	}
}