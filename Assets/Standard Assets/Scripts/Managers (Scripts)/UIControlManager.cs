using UnityEngine;
using UnityEngine.UI;
using Extensions;
using ArcherGame;

public class UIControlManager : SingletonMonoBehaviour<UIControlManager>, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public _Selectable currentSelected;
	public ComplexTimer colorMultiplier;
	public static _Selectable[] selectables = new _Selectable[0];
	Vector2 inputDirection;
	Vector2 previousInputDirection;
	public Timer repeatTimer;
	public float angleEffectiveness;
	public float distanceEffectiveness;
	_Selectable selectable;
	bool inControlMode;
	bool controllingWithJoystick;
	bool leftClickInput;
	bool previousLeftClickInput;
	InputField currentInputField;

	public virtual void Awake ()
	{
		GameManager.updatables = GameManager.updatables.Add(this);
		repeatTimer.onFinished += delegate { _HandleChangeSelected (); ControlSelected (); };
	}

	public virtual void OnDestroy ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
		repeatTimer.onFinished -= delegate { _HandleChangeSelected (); ControlSelected (); };
	}

	public virtual void DoUpdate ()
	{
		leftClickInput = InputManager.GetLeftClickInput(MathfExtensions.NULL_INT);
		if (currentSelected != null)
		{
			if (!CanSelectSelectable(currentSelected))
			{
				ColorSelected (currentSelected, 1);
				HandleChangeSelected (false);
			}
			ColorSelected (currentSelected, colorMultiplier.GetValue());
			HandleMouseInput ();
			HandleMovementInput ();
			HandleSubmitSelected ();
		}
		else
			HandleChangeSelected (false);
		previousLeftClickInput = leftClickInput;
	}

	public virtual bool CanSelectSelectable (_Selectable selectable)
	{
		return selectables.Contains(selectable) && selectable.selectable.IsInteractable() && selectable.canvas.enabled;
	}

	public virtual bool IsMousedOverSelectable (_Selectable selectable)
	{
		return IsMousedOverRectTransform(selectable.rectTrs, selectable.canvas, selectable.canvasRectTrs);
	}

	public virtual bool IsMousedOverRectTransform (RectTransform rectTrs, Canvas canvas, RectTransform canvasRectTrs)
	{
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.worldCamera == null)
			return rectTrs.GetRectInCanvasNormalized(canvasRectTrs).Contains(canvasRectTrs.GetWorldRect().ToNormalizedPosition(InputManager.GetMousePosition(MathfExtensions.NULL_INT)));
		else
			return rectTrs.GetRectInCanvasNormalized(canvasRectTrs).Contains(canvasRectTrs.GetWorldRect().ToNormalizedPosition(canvas.worldCamera.ScreenToWorldPoint(InputManager.GetMousePosition(MathfExtensions.NULL_INT))));
	}

	public virtual void HandleMouseInput ()
	{
		if (InputManager.UsingGamepad)
			return;
		if (!leftClickInput && previousLeftClickInput && !controllingWithJoystick)
			inControlMode = false;
		foreach (_Selectable selectable in selectables)
		{
			if (currentSelected != selectable && IsMousedOverSelectable(selectable) && CanSelectSelectable(selectable))
			{
				ChangeSelected (selectable);
				return;
			}
		}
		if (leftClickInput)
		{
			if (currentInputField != null)
				currentInputField.readOnly = true;
			_Slider slider = currentSelected.GetComponent<_Slider>();
			if (slider != null)
			{
				Vector2 closestPointToMouseCanvasNormalized = new Vector2();
				if (currentSelected.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					if (selectable != null)
						closestPointToMouseCanvasNormalized = slider.slidingArea.GetRectInCanvasNormalized(selectable.canvasRectTrs).ClosestPoint(slider.canvasRectTrs.GetWorldRect().ToNormalizedPosition(InputManager.GetMousePosition(MathfExtensions.NULL_INT)));
				}
				else if (selectable != null)
					closestPointToMouseCanvasNormalized = slider.slidingArea.GetRectInCanvasNormalized(selectable.canvasRectTrs).ClosestPoint(slider.canvasRectTrs.GetWorldRect().ToNormalizedPosition(selectable.canvas.worldCamera.ScreenToWorldPoint(InputManager.GetMousePosition(MathfExtensions.NULL_INT))));
				float normalizedValue = slider.slidingArea.GetRectInCanvasNormalized(slider.canvasRectTrs).ToNormalizedPosition(closestPointToMouseCanvasNormalized).x;
				slider.slider.value = Mathf.Lerp(slider.slider.minValue, slider.slider.maxValue, normalizedValue);
				if (slider.snapValues.Length > 0)
					slider.slider.value = MathfExtensions.GetClosestNumber(slider.slider.value, slider.snapValues);
			}
			else
			{
				InputField inputField = currentSelected.GetComponent<InputField>();
				if (inputField != null)
				{
					currentInputField = inputField;
					currentInputField.readOnly = false;
				}
			}
		}
	}

	public virtual void HandleMovementInput ()
	{
		inputDirection = InputManager.GetUIMovementInput(MathfExtensions.NULL_INT);
		if (inputDirection.magnitude > InputManager.Settings.defaultDeadzoneMin)
		{
			if (previousInputDirection.magnitude <= InputManager.Settings.defaultDeadzoneMin)
			{
				HandleChangeSelected (true);
				ControlSelected ();
				repeatTimer.Reset ();
				repeatTimer.Start ();
			}
		}
		else
			repeatTimer.Stop ();
		previousInputDirection = inputDirection;
	}

	public virtual void _HandleChangeSelected ()
	{
		HandleChangeSelected (true);
	}

	public virtual void HandleChangeSelected (bool useInputDirection = true)
	{
		if (selectables.Length == 0 || inControlMode || (currentInputField != null && !currentInputField.readOnly))
			return;
		_Selectable[] otherSelectables = new _Selectable[0];
		otherSelectables = otherSelectables.AddRange(selectables);
		otherSelectables = otherSelectables.Remove(currentSelected);
		if (otherSelectables.Length == 0)
			return;
		float selectableAttractiveness;
		_Selectable nextSelected = otherSelectables[0];
		float highestSelectableAttractiveness = GetAttractivenessOfSelectable(nextSelected, useInputDirection);
		for (int i = 1; i < otherSelectables.Length; i ++)
		{
			_Selectable selectable = otherSelectables[i];
			selectableAttractiveness = GetAttractivenessOfSelectable(selectable, useInputDirection);
			if (selectableAttractiveness > highestSelectableAttractiveness)
			{
				highestSelectableAttractiveness = selectableAttractiveness;
				nextSelected = selectable;
			}
		}
		ChangeSelected (nextSelected);
	}

	public virtual void ChangeSelected (_Selectable selectable)
	{
		if (inControlMode)
			return;
		if (currentSelected != null)
			ColorSelected (currentSelected, 1);
		currentSelected = selectable;
		currentSelected.selectable.Select();
		colorMultiplier.JumpToStart ();
	}

	public virtual void HandleSubmitSelected ()
	{
		bool submitInput = InputManager.GetSubmitInput(MathfExtensions.NULL_INT);
		if (currentSelected.gameObject.activeInHierarchy && (submitInput || (IsMousedOverSelectable(currentSelected) && leftClickInput && !previousLeftClickInput)))
		{
			Button button = currentSelected.GetComponent<Button>();
			if (button != null)
				button.onClick.Invoke();
			else
			{
				Toggle toggle = currentSelected.GetComponent<Toggle>();
				if (toggle != null)
					toggle.isOn = !toggle.isOn;
				else
				{
					_Slider slider = currentSelected.GetComponent<_Slider>();
					if (slider != null)
					{
						controllingWithJoystick = submitInput;
						inControlMode = !inControlMode;
					}
				}
			}
		}
	}

	public virtual void ControlSelected ()
	{
		if (!inControlMode)
			return;
		_Slider slider = currentSelected.GetComponent<_Slider>();
		if (slider != null)
		{
			slider.indexOfCurrentSnapValue = Mathf.Clamp(slider.indexOfCurrentSnapValue + MathfExtensions.Sign(inputDirection.x), 0, slider.snapValues.Length - 1);
			slider.slider.value = slider.snapValues[slider.indexOfCurrentSnapValue];
		}
	}

	public virtual float GetAttractivenessOfSelectable (_Selectable selectable, bool useInputDirection = true)
	{
		if (!CanSelectSelectable(selectable))
			return -Mathf.Infinity;
		float attractiveness = selectable.priority;
		if (useInputDirection)
		{
			Vector2 directionToSelectable = GetDirectionToSelectable(selectable);
			float angleAttractiveness = (180f - Vector2.Angle(inputDirection, directionToSelectable)) * angleEffectiveness;
			float distanceAttractiveness = directionToSelectable.magnitude * distanceEffectiveness;
			attractiveness += angleAttractiveness - distanceAttractiveness;
		}
		return attractiveness;
	}

	public virtual Vector2 GetDirectionToSelectable (_Selectable selectable)
	{
		return selectable.rectTrs.GetCenterInCanvasNormalized(selectable.canvasRectTrs) - currentSelected.rectTrs.GetCenterInCanvasNormalized(currentSelected.canvasRectTrs);
	}

	public virtual void ColorSelected (_Selectable selectable, float colorMultiplier)
	{
		ColorBlock colors = selectable.selectable.colors;
		colors.colorMultiplier = colorMultiplier;
		selectable.selectable.colors = colors;
	}
}