using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ClampedFloat
{
	public float value;
	public FloatRange valueRange;
	
	public float GetValue ()
	{
		return Mathf.Clamp(value, valueRange.min, valueRange.max);
	}
	
	public void SetValue (float value)
	{
		this.value = value;
		value = GetValue();
	}
}