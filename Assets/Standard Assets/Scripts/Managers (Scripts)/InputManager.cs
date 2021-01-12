using UnityEngine;
using Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ArcherGame
{
	public class InputManager : SingletonMonoBehaviour<InputManager>
	{
		public InputSettings settings;
		public static InputSettings Settings
		{
			get
			{
				return InputManager.Instance.settings;
			}
		}
		public static bool UsingGamepad
		{
			get
			{
				return Gamepad.current != null;
			}
		}
		public static bool UsingMouse
		{
			get
			{
				return Mouse.current != null;
			}
		}
		public static bool UsingKeyboard
		{
			get
			{
				return Keyboard.current != null;
			}
		}
		public float MoveInput
		{
			get
			{
				return GetMoveInput(MathfExtensions.NULL_INT);
			}
		}
		public Vector2 SwimInput
		{
			get
			{
				return GetSwimInput(MathfExtensions.NULL_INT);
			}
		}
		public bool JumpInput
		{
			get
			{
				return GetJumpInput(MathfExtensions.NULL_INT);
			}
		}
		public bool InteractInput
		{
			get
			{
				return GetInteractInput(MathfExtensions.NULL_INT);
			}
		}
		public float ZoomInput
		{
			get
			{
				return GetZoomInput(MathfExtensions.NULL_INT);
			}
		}
		public bool SubmitInput
		{
			get
			{
				return GetSubmitInput(MathfExtensions.NULL_INT);
			}
		}
		public bool ArrowActionInput
		{
			get
			{
				return GetArrowActionInput(MathfExtensions.NULL_INT);
			}
		}
		public int SwitchArrowInput
		{
			get
			{
				return GetSwitchArrowInput(MathfExtensions.NULL_INT);
			}
		}
		public bool ShootInput
		{
			get
			{
				return GetShootInput(MathfExtensions.NULL_INT);
			}
		}
		public bool ArrowMenuInput
		{
			get
			{
				return GetArrowMenuInput(MathfExtensions.NULL_INT);
			}
		}
		public int SwitchMenuSectionInput
		{
			get
			{
				return GetSwitchMenuSectionInput(MathfExtensions.NULL_INT);
			}
		}
		public Vector2 AimInput
		{
			get
			{
				return GetAimInput(MathfExtensions.NULL_INT);
			}
		}
		public Vector2 UIMovementInput
		{
			get
			{
				return GetUIMovementInput(MathfExtensions.NULL_INT);
			}
		}
		public bool PauseInput
		{
			get
			{
				return GetPauseInput(MathfExtensions.NULL_INT);
			}
		}
		
		public virtual void Start ()
		{
			InputSystem.onDeviceChange += OnDeviceChanged;
		}
		
		public virtual void OnDeviceChanged (InputDevice device, InputDeviceChange change)
		{
			if (device is Gamepad)
			{
				if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
				{
					GameManager.activeCursorEntry.rectTrs.gameObject.SetActive(false);
					if (VirtualKeyboard.Instance != null)
						VirtualKeyboard.instance.outputToInputField.readOnly = true;
				}
				else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
				{
					GameManager.activeCursorEntry.rectTrs.gameObject.SetActive(true);
					if (VirtualKeyboard.Instance != null)
						VirtualKeyboard.instance.outputToInputField.readOnly = false;
				}
				foreach (_Text text in _Text.instances)
					text.UpdateText ();
			}
		}
		
		public virtual void OnDestroy ()
		{
			InputSystem.onDeviceChange -= OnDeviceChanged;
		}

		public static float GetMoveInput (int playerIndex)
		{
			if (UsingGamepad)
				return GetGamepad(playerIndex).leftStick.x.ReadValue();
			else
			{
				int output = 0;
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				if (Input.GetKey(KeyCode.D))
					output ++;
				if (Input.GetKey(KeyCode.A))
					output --;
				return output;
#endif
				Keyboard keyboard = GetKeyboard(playerIndex);
				if (keyboard.dKey.isPressed)
					output ++;
				if (keyboard.aKey.isPressed)
					output --;
				return output;
			}
		}

		public static Vector2 GetSwimInput (int playerIndex)
		{
			if (UsingGamepad)
				return Vector2.ClampMagnitude(GetGamepad(playerIndex).leftStick.ReadValue(), 1);
			else
			{
				int y = 0;
				Keyboard keyboard = GetKeyboard(playerIndex);
				if (keyboard.wKey.isPressed)
					y ++;
				if (keyboard.sKey.isPressed)
					y --;
				return Vector2.ClampMagnitude(new Vector2(GetMoveInput(playerIndex), y), 1);
			}
		}

		public static bool GetJumpInput (int playerIndex)
		{
			if (UsingGamepad)
				return GetGamepad(playerIndex).leftTrigger.isPressed;
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.W);
#endif
				return GetKeyboard(playerIndex).wKey.isPressed;
		}

		public static bool GetInteractInput (int playerIndex)
		{
			if (UsingGamepad)
				return GetGamepad(playerIndex).aButton.isPressed;
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.E);
#endif
				return GetKeyboard(playerIndex).eKey.isPressed;
		}

		public static float GetZoomInput (int playerIndex)
		{
			if (UsingGamepad)
				return GetGamepad(playerIndex).rightStick.y.ReadValue();
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.mouseScrollDelta.y;
#endif
				return GetMouse(playerIndex).scroll.y.ReadValue();
		}

		public static bool GetSubmitInput (int playerIndex)
		{
			if (UsingGamepad)
				return GetGamepad(playerIndex).aButton.isPressed;
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.Return);
#endif
				return GetKeyboard(playerIndex).enterKey.isPressed;
		}

		public static bool GetArrowActionInput (int playerIndex)
		{
			if (UsingGamepad)
				return GetGamepad(playerIndex).rightTrigger.isPressed;
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.Mouse0);
#endif
				return GetMouse(playerIndex).leftButton.isPressed;
		}

		public static int GetSwitchArrowInput (int playerIndex)
		{
			if (UsingGamepad)
				return -1;
			else
			{
				Keyboard keyboard = GetKeyboard(playerIndex); 
				if (keyboard.spaceKey.isPressed)
					return 0;
				else if (keyboard.digit1Key.isPressed)
					return 1;
				else if (keyboard.digit2Key.isPressed)
					return 2;
				else if (keyboard.digit3Key.isPressed)
					return 3;
				else if (keyboard.digit4Key.isPressed)
					return 4;
				else
					return -1;
			}
		}


		public static bool GetShootInput (int playerIndex)
		{
			if (UsingGamepad)
			{
				Gamepad gamepad = GetGamepad(playerIndex);
				return gamepad.rightTrigger.isPressed || gamepad.rightStickButton.isPressed;
			}
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.Mouse0);
#endif
				return GetMouse(playerIndex).leftButton.isPressed;
		}
		
		public static bool GetArrowMenuInput (int playerIndex)
		{
			if (UsingGamepad)
			{
				Gamepad gamepad = GetGamepad(playerIndex);
				return gamepad.rightShoulder.isPressed || gamepad.leftShoulder.isPressed;
			}
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.Mouse1);
#endif
				return GetMouse(playerIndex).rightButton.isPressed;
		}

		public static int GetSwitchMenuSectionInput (int playerIndex)
		{
			if (UsingGamepad)
			{
				int output = 0;
				Gamepad gamepad = GetGamepad(playerIndex);
				if (gamepad.rightShoulder.isPressed)
					output ++;
				if (gamepad.leftShoulder.isPressed)
					output --;
				return output;
			}
			else
				return 0;
		}

		public static Vector2 GetAimInput (int playerIndex)
		{
			if (UsingGamepad)
				return Vector2.ClampMagnitude(GetGamepad(playerIndex).rightStick.ReadValue(), 1);
			else
				return Vector2.ClampMagnitude(GameCamera.Instance.camera.ScreenToWorldPoint(GetMousePosition(playerIndex)) - Player.instance.trs.position, 1);
		}

		public static Vector2 GetUIMovementInput (int playerIndex)
		{
			if (UsingGamepad)
				return Vector2.ClampMagnitude(GetGamepad(playerIndex).leftStick.ReadValue(), 1);
			else
			{
				Keyboard keyboard = GetKeyboard(playerIndex);
				int x = 0;
				if (keyboard.dKey.isPressed)
					x ++;
				if (keyboard.aKey.isPressed)
					x --;
				int y = 0;
				if (keyboard.wKey.isPressed)
					y ++;
				if (keyboard.sKey.isPressed)
					y --;
				return Vector2.ClampMagnitude(new Vector2(x, y), 1);
			}
		}

		public static bool GetPauseInput (int playerIndex)
		{
			if (UsingGamepad)
			{
				Gamepad gamepad = GetGamepad(playerIndex);
				return gamepad.startButton.isPressed || gamepad.selectButton.isPressed;
			}
			else
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Input.GetKey(KeyCode.Escape);
#endif
				return GetKeyboard(playerIndex).escapeKey.isPressed;
		}

		public static bool GetLeftClickInput (int playerIndex)
		{
			if (UsingMouse)
				return GetMouse(playerIndex).leftButton.isPressed;
			else
				return false;
		}

		public static Vector2 GetMousePosition (int playerIndex)
		{
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
			return Input.mousePosition;
#endif
			if (UsingMouse)
				return GetMouse(playerIndex).position.ReadValue();
			else
				return GameManager.activeCursorEntry.rectTrs.position;
		}

		public static Vector2 GetWorldMousePosition (int playerIndex)
		{
			Rect gameViewRect = GameManager.Instance.gameViewRectTrs.GetWorldRect();
			return CameraScript.Instance.camera.ViewportToWorldPoint(GetMousePosition(playerIndex));
		}

		public static Gamepad GetGamepad (int playerIndex)
		{
			Gamepad gamepad = Gamepad.current;
			if (Gamepad.all.Count > playerIndex)
				gamepad = Gamepad.all[playerIndex];
			return gamepad;
		}

		public static Mouse GetMouse (int playerIndex)
		{
			Mouse mouse = Mouse.current;
			// if (Mouse.all.Count > playerIndex)
			// 	mouse = (Mouse) Mouse.all[playerIndex];
			return mouse;
		}

		public static Keyboard GetKeyboard (int playerIndex)
		{
			Keyboard keyboard = Keyboard.current;
			// if (Keyboard.all.Count > playerIndex)
			// 	keyboard = (Keyboard) Keyboard.all[playerIndex];
			return keyboard;
		}
	}
}