using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ArcherGame;
using Extensions;

public class ComplexTimer : MonoBehaviour, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public string title;
	public bool realtime;
	public ClampedFloat value;
	public float changeAmountMultiplier;
	public RepeatType repeatType;
	float timeSinceLastGetValue;
	float previousChangeAmountMultiplier;
	[HideInInspector]
	public float initValue;
	
	public virtual float GetValue ()
	{
		value.SetValue(value.GetValue() + timeSinceLastGetValue * changeAmountMultiplier);
		if ((value.GetValue() == value.valueRange.max && changeAmountMultiplier > 0) || (value.GetValue() == value.valueRange.min && changeAmountMultiplier < 0))
		{
			if (repeatType == RepeatType.Loop)
				JumpToStart ();
			else if (repeatType  == RepeatType.PingPong)
				changeAmountMultiplier *= -1;
		}
		timeSinceLastGetValue = 0;
		return value.GetValue();
	}
	
	public virtual void Awake ()
	{
#if UNITY_EDTIOR
		if (!Application.isPlaying)
			return;
#endif
		if (value == null)
			value = new ClampedFloat();
		initValue = value.GetValue();
		GameManager.updatables = GameManager.updatables.Add(this);
	}
	
	public virtual void DoUpdate ()
	{
		if (realtime)
			UpdateTimer(Time.unscaledDeltaTime);
		else
			UpdateTimer(Time.deltaTime);
	}

	public virtual void OnDestroy ()
	{
#if UNITY_EDTIOR
		if (!Application.isPlaying)
			return;
#endif
		GameManager.updatables = GameManager.updatables.Remove(this);
	}
	
	public virtual void UpdateTimer (float deltaTime)
	{
		timeSinceLastGetValue += deltaTime;
	}
	
	public virtual bool IsAtStart ()
	{
		float timerValue = GetValue();
		return (timerValue == value.valueRange.max && changeAmountMultiplier < 0) || (timerValue == value.valueRange.min && changeAmountMultiplier > 0) || ((timerValue == value.valueRange.min || timerValue == value.valueRange.max) && changeAmountMultiplier == 0);
	}
	
	public virtual bool IsAtEnd ()
	{
		float timerValue = GetValue();
		return (timerValue == value.valueRange.min && changeAmountMultiplier < 0) || (timerValue == value.valueRange.max && changeAmountMultiplier > 0) || ((timerValue == value.valueRange.min || timerValue == value.valueRange.max) && changeAmountMultiplier == 0);
	}
	
	public virtual void Pause ()
	{
		if (changeAmountMultiplier == 0)
			return;
		previousChangeAmountMultiplier = changeAmountMultiplier;
		changeAmountMultiplier = 0;
	}
	
	public virtual void Resume ()
	{
		if (changeAmountMultiplier != 0)
			return;
		changeAmountMultiplier = previousChangeAmountMultiplier;
	}
	
	public virtual void JumpToStart ()
	{
		if (changeAmountMultiplier > 0 || (changeAmountMultiplier == 0 && previousChangeAmountMultiplier > 0))
			value.SetValue(value.valueRange.min);
		else
			value.SetValue(value.valueRange.max);
	}
	
	public virtual void JumpToEnd ()
	{
		if (changeAmountMultiplier > 0 || (changeAmountMultiplier == 0 && previousChangeAmountMultiplier > 0))
			value.SetValue(value.valueRange.max);
		else
			value.SetValue(value.valueRange.min);
	}
	
	public virtual void JumpToInitValue ()
	{
		value.SetValue(initValue);
	}
	
	public enum RepeatType
	{
		End = 0,
		Loop = 1,
		PingPong = 2
	}
}