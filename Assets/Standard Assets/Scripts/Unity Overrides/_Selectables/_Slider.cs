using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Extensions;
using ArcherGame;

[RequireComponent(typeof(Slider))]
public class _Slider : _Selectable
{
	public Slider slider;
	public Text displayValue;
	string initDisplayValue;
	public float[] snapValues;
	[HideInInspector]
	public int indexOfCurrentSnapValue;
	public RectTransform slidingArea;
	
	public virtual void Awake ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		if (displayValue != null)
			initDisplayValue = displayValue.text;
		SetDisplayValue ();
		if (snapValues.Length > 0)
			indexOfCurrentSnapValue = MathfExtensions.GetIndexOfClosestNumber(slider.value, snapValues);
		slider.onValueChanged.AddListener(delegate { OnValueChanged (slider.value); });
	}

	public override void OnEnable ()
	{
		base.OnEnable ();
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (slidingArea == null)
			{
				slidingArea = GetComponent<RectTransform>();
				slidingArea = slidingArea.Find("Handle Slide Area") as RectTransform;
			}
			if (rectTrs == null)
			{
				rectTrs = GetComponent<RectTransform>();
				rectTrs = rectTrs.Find("Handle") as RectTransform;
			}
		}
#endif
	}

	public virtual void OnValueChanged (float value)
	{
		if (snapValues.Length > 0)
			slider.value = MathfExtensions.GetClosestNumber(value, snapValues);
		 SetDisplayValue ();
	}
	
	public virtual void SetDisplayValue ()
	{
		displayValue.text = initDisplayValue + slider.value;
	}
}