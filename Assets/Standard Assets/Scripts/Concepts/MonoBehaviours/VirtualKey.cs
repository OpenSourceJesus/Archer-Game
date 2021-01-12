using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Extensions;
using ArcherGame;

//[ExecuteInEditMode]
public class VirtualKey : MonoBehaviour, IUpdatable
{
	public Text text;
	public Outline outline;
	public Image image;
	bool isSelected;
	[HideInInspector]
	public Color notSelectedColor;
	public Color selectedColor;
	[HideInInspector]
	public Color notPressedColor;
	public Color pressedColor;
	public bool isActivatable;
	[HideInInspector]
	public bool isActivated;
	public Color activeColor;
	public UnityEvent invokeOnPressed;
	public UnityEvent invokeOnReleased;
	[HideInInspector]
	public string normalText;
	public bool alternateTextSameAsNormal;
	public string alternateText;
	[HideInInspector]
	public bool isPressed;
	public _Selectable selectable;
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}

	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			notSelectedColor = outline.effectColor;
			notPressedColor = image.color;
			normalText = text.text;
			if (alternateTextSameAsNormal)
				alternateText = normalText;
			return;
		}
#endif
		previousSubmitInput = true;
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	bool submitInput;
	bool previousSubmitInput;
	bool leftClickInput;
	bool previousLeftClickInput;
	public virtual void DoUpdate ()
	{
		submitInput = InputManager.GetSubmitInput(MathfExtensions.NULL_INT);
		leftClickInput = InputManager.GetLeftClickInput(MathfExtensions.NULL_INT);
		isSelected = UIControlManager.Instance.currentSelected == selectable;
		if (isSelected)
		{
			outline.effectColor = selectedColor;
			if ((submitInput && !previousSubmitInput) || (UIControlManager.Instance.IsMousedOverSelectable(selectable) && leftClickInput && !previousLeftClickInput))
				OnPressed ();
			else if (isPressed && ((!submitInput && previousSubmitInput) || (!leftClickInput && previousLeftClickInput)))
				OnReleased ();
		}
		else
		{
			outline.effectColor = notSelectedColor;
			if (isPressed)
				OnReleased ();
		}
		previousSubmitInput = submitInput;
		previousLeftClickInput = leftClickInput;
	}

	public virtual void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

	public virtual void OnPressed ()
	{
		if (isPressed && isSelected)
			return;
		isPressed = true;
		image.color = pressedColor;
		if (invokeOnPressed != null)
			invokeOnPressed.Invoke(); 
	}

	public virtual void OnReleased ()
	{
		if (!isPressed)
			return;
		isPressed = false;
		image.color = notPressedColor;
		if (isActivatable)
			ToggleActivate ();
		if (invokeOnReleased != null)
			invokeOnReleased.Invoke();
	}

	public virtual void ToggleActivate ()
	{
		isActivated = !isActivated;
		if (isActivated)
			image.color = activeColor;
		else
			image.color = notPressedColor;
	}

	public virtual void StartContinuousOutputToInputField ()
	{
		StartCoroutine(ContinuousOutputToInputFieldRoutine ());
	}

	public virtual IEnumerator ContinuousOutputToInputFieldRoutine ()
	{
		OutputToInputField ();
		yield return new WaitForSecondsRealtime(VirtualKeyboard.Instance.repeatDelay);
		while (true)
		{
			yield return new WaitForSecondsRealtime(VirtualKeyboard.instance.repeatRate);
			OutputToInputField ();
		}
	}

	public virtual void OutputToInputField ()
	{
		if (VirtualKeyboard.Instance.outputToInputField.text.Length == VirtualKeyboard.instance.outputToInputField.characterLimit)
			return;
		VirtualKeyboard.instance.outputToInputField.text = VirtualKeyboard.instance.outputToInputField.text.Insert(VirtualKeyboard.instance.OutputPosition, text.text);
		VirtualKeyboard.instance.OutputPosition ++;
		if (VirtualKeyboard.instance.shiftKeys[0].isActivated)
		{
			VirtualKeyboard.instance.shiftKeys[0].ToggleActivate ();
			VirtualKeyboard.instance.OnShiftReleased (VirtualKeyboard.instance.shiftKeys[0]);
		}
	}

	public virtual void EndContinuousOutputToInputField ()
	{
		StopAllCoroutines();
	}
}