using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPIe;
using UnityEngine.UI;
using Extensions;
using System;
using UnityEngine.InputSystem;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class CircularMenu : MonoBehaviour, IUpdatable
	{
		public RectTransform rectTrs;
		public CanvasGroup canvasGroup;
		public RectTransform canvasRectTrs;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		MenuOption defaultOption;
		public MenuOption[] options = new MenuOption[0];
		MenuOption currentOption;
		MenuOption previousOption;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (rectTrs == null)
					rectTrs = GetComponent<RectTransform>();
				if (canvasGroup == null)
					canvasGroup = GetComponent<CanvasGroup>();
				if (options.Length == 0)
				{
					Button[] buttons = GetComponentsInChildren<Button>();
					foreach (Button button in buttons)
					{
						MenuOption menuOption = new MenuOption();
						menuOption.button = button;
						menuOption.rectTrs = button.GetComponent<RectTransform>();
						options = options.Add(menuOption);
					}
				}
				return;
			}
#endif
			defaultOption = options[0];
			gameObject.SetActive(false);
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		bool arrowMenuInput;
		bool previousArrowMenuInput;
		public virtual void DoUpdate ()
		{
			arrowMenuInput = InputManager.GetArrowMenuInput(MathfExtensions.NULL_INT);
			if (arrowMenuInput && !previousArrowMenuInput)
			{
				if (InputManager.UsingGamepad)
					rectTrs.localPosition = Vector2.zero;
				else
					rectTrs.anchoredPosition = GetMousePositionOnCanvas();
				previousOption = options[0];
				SetOptionColorMultiplier (previousOption, 0.5f);
				gameObject.SetActive(true);
			}
			else if (gameObject.activeSelf)
			{
				if (InputManager.UsingGamepad)
					currentOption = GetOptionAtDirection(InputManager.GetAimInput(MathfExtensions.NULL_INT));
				else
					currentOption = GetOptionAtDirection(GetMousePositionOnCanvas() - (Vector2) rectTrs.anchoredPosition);
				if (currentOption != previousOption)
				{
					SetOptionColorMultiplier (currentOption, 0.5f);
					SetOptionColorMultiplier (previousOption, 1);
					previousOption = currentOption;
				}
			}
			if (!arrowMenuInput && previousArrowMenuInput)
			{
				gameObject.SetActive(false);
				SetOptionColorMultiplier (currentOption, 1);
				currentOption.button.onClick.Invoke();
			}
			previousArrowMenuInput = arrowMenuInput;
		}

		public virtual Vector2 GetMousePositionOnCanvas ()
		{
			return canvasRectTrs.sizeDelta.Multiply(GameCamera.Instance.camera.ScreenToViewportPoint(InputManager.GetMousePosition(MathfExtensions.NULL_INT)));
		}

		public virtual MenuOption GetOptionAtDirection (Vector2 direction)
		{
			float degreesPerOption = 360f / (options.Length - 1);
			MenuOption[] availableOptions = (MenuOption[]) options.Clone();
			if (!GameManager.ModifierIsActiveAndExists("Default Circular Menu Option Is Center Pixel"))
			{
				if (InputManager.UsingGamepad)
				{
					if (direction.magnitude <= defaultOption.rectTrs.GetWorldRect().width / rectTrs.GetWorldRect().width)
						return defaultOption;
				}
				else
				{
					if (direction.magnitude <= defaultOption.rectTrs.sizeDelta.x / 2 * rectTrs.localScale.x)
						return defaultOption;
				}
			}
			availableOptions = availableOptions.Remove(defaultOption);
			MenuOption option;
			for (int i = 0; i < availableOptions.Length; i ++)
			{
				option = availableOptions[i];
				if (Vector2.Angle(option.rectTrs.localPosition, direction) <= degreesPerOption / 2)
				{
					if (GameManager.ModifierIsActiveAndExists("Default Circular Menu Option Is Center Pixel"))
						return option;
					if (InputManager.UsingGamepad)
					{
						if (direction.magnitude > defaultOption.rectTrs.GetWorldRect().width / rectTrs.GetWorldRect().width)
							return option;
					}
					else
					{
						if (direction.magnitude > defaultOption.rectTrs.sizeDelta.x / 2 * rectTrs.localScale.x)
							return option;
					}
				}
			}
			return null;
		}

		public virtual void SetOptionColorMultiplier (MenuOption option, float colorMultiplier)
		{
			ColorBlock colors = option.button.colors;
			colors.colorMultiplier = colorMultiplier;
			option.button.colors = colors;
		}

		[Serializable]
		public class MenuOption
		{
			public Button button;
			public RectTransform rectTrs;
		}
	}
}