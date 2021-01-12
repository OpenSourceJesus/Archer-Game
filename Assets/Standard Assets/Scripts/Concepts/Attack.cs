using System;
using UnityEngine;
using Extensions;
using System.Collections;
using System.Collections.Generic;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class Attack : ScriptableObject, IConfigurable
	{
		public float activateRange;
		public float activateRangeSqr;
		public string attackTriggerName;
		public string exitTriggerName;
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
				return "Attacks";
			}
		}
		
		public virtual void Awake ()
		{
			activateRangeSqr = activateRange * activateRange;
		}
		
		public virtual void Do (InstanceData data)
		{
			GameManager.Instance.StartCoroutine(DoRoutine (data));
		}

		public virtual IEnumerator DoRoutine (InstanceData data)
		{
			if (data.awakableEnemy.animator != null)
			{
				data.awakableEnemy.animator.ResetTrigger(exitTriggerName);
				data.awakableEnemy.animator.SetTrigger(attackTriggerName);
			}
			Aim (data);
			if (data.tempActiveObject != null)
				yield return GameManager.Instance.StartCoroutine(data.tempActiveObject.DoRoutine ());
			if (data.awakableEnemy.animator != null)
			{
				data.awakableEnemy.animator.ResetTrigger(attackTriggerName);
				data.awakableEnemy.animator.SetTrigger(exitTriggerName);
			}
		}

		public virtual bool ShouldDo (InstanceData data)
		{
			if (data.awakableEnemy == null || data.awakableEnemy.isDead || (data.tempActiveObject.obj != null && data.tempActiveObject.obj.activeSelf))
				return false;
			return (Player.instance.trs.position - data.awakableEnemy.trs.position).sqrMagnitude <= activateRangeSqr;
		}

		public virtual void Aim (InstanceData data)
		{
		}

		[Serializable]
		public class InstanceData
		{
			public AwakableEnemy awakableEnemy;
			public TemporaryActiveObject tempActiveObject;
		}
	}
}