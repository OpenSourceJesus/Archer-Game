using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Extensions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using DialogAndStory;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class GameManager : SingletonMonoBehaviour<GameManager>, ISaveableAndLoadable
	{
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
		public static bool HasPlayedBefore
		{
			get
			{
				return PlayerPrefs.GetInt("Has played before", 0) == 1;
			}
			set
			{
				PlayerPrefs.SetInt("Has played before", value.GetHashCode());
			}
		}
		public static float UnscaledDeltaTime
		{
			get
			{
				if (paused || framesSinceLoadedScene <= LAG_FRAMES_AFTER_LOAD_SCENE)
					return 0;
				else
					return Time.unscaledDeltaTime;
			}
		}
		public static bool paused;
		public GameObject[] registeredGos = new GameObject[0];
		[SaveAndLoadValue(false)]
		public static string enabledGosString = "";
		[SaveAndLoadValue(false)]
		public static string disabledGosString = "";
		public const string STRING_SEPERATOR = "|";
		public float timeScale;
		public Team[] teams;
		public static IUpdatable[] updatables = new IUpdatable[0];
		public static IUpdatable[] pausedUpdatables = new IUpdatable[0];
		public static Dictionary<Type, object> singletons = new Dictionary<Type, object>();
		public const char UNIQUE_ID_SEPERATOR = ',';
#if UNITY_EDITOR
		public static int[] UniqueIds
		{
			get
			{
				int[] output = new int[0];
				string[] uniqueIdsString = EditorPrefs.GetString("Unique ids").Split(UNIQUE_ID_SEPERATOR);
				int uniqueIdParsed;
				foreach (string uniqueIdString in uniqueIdsString)
				{
					if (int.TryParse(uniqueIdString, out uniqueIdParsed))
						output = output.Add(uniqueIdParsed);
				}
				return output;
			}
			set
			{
				string uniqueIdString = "";
				foreach (int uniqueId in value)
					uniqueIdString += uniqueId + UNIQUE_ID_SEPERATOR;
				EditorPrefs.SetString("Unique ids", uniqueIdString);
			}
		}
		public bool doEditorUpdates;
#endif
		public static int framesSinceLoadedScene;
		public const int LAG_FRAMES_AFTER_LOAD_SCENE = 2;
		public Animator screenEffectAnimator;
		public CursorEntry[] cursorEntries;
		public static Dictionary<string, CursorEntry> cursorEntriesDict = new Dictionary<string, CursorEntry>();
		public static CursorEntry activeCursorEntry;
		public RectTransform cursorCanvasRectTrs;
		public GameModifier[] gameModifiers;
		public static Dictionary<string, GameModifier> gameModifierDict = new Dictionary<string, GameModifier>();
		public float cursorMoveSpeedPerScreenArea;
		public static float cursorMoveSpeed;
		public static Vector2 previousMousePosition;
		public delegate void OnGameScenesLoaded();
		public static event OnGameScenesLoaded onGameScenesLoaded;
		public static bool isFocused;
		static bool initialized;
		public GameObject emptyGoPrefab;
		public Conversation worldMapTutorialConversation;
		public Dialog worldMapMoveViewTutorialDialog;
		public Dialog worldMapZoomViewTutorialDialog;
		public Conversation movementJumpingShootingTutorialConversation;
		public Dialog movementTutorialDialog;
		public Dialog jumpingTutorialDialog;
		public Dialog shootingTutorialDialog;
		public Dialog arrowMenuTutorialDialog;
		public TemporaryActiveText notificationText;
		public Timer hideCursorTimer;
		public GameScene[] gameScenes;
		public Canvas[] canvases = new Canvas[0];
		public RectTransform gameViewRectTrs;
		public Canvas gameViewCanvas;
		public GameObject screenBlockerGo;
		Vector2 mousePosition;
		Vector2 moveInput;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				framesSinceLoadedScene = 0;
				Transform[] transforms = FindObjectsOfType<Transform>();
				IIdentifiable[] identifiables = new IIdentifiable[0];
				foreach (Transform trs in transforms)
				{
					identifiables = trs.GetComponents<IIdentifiable>();
					foreach (IIdentifiable identifiable in identifiables)
					{
						if (!UniqueIds.Contains(identifiable.UniqueId))
							UniqueIds = UniqueIds.Add(identifiable.UniqueId);
					}
				}
				return;
			}
			// else
			// {
			// 	for (int i = 0; i < gameScenes.Length; i ++)
			// 	{
			// 		if (!gameScenes[i].use)
			// 		{
			// 			gameScenes = gameScenes.RemoveAt(i);
			// 			i --;
			// 		}
			// 	}
			// }
#endif
			base.Awake ();
			if (singletons.ContainsKey(GetType()))
				singletons[GetType()] = this;
			else
				singletons.Add(GetType(), this);
			InitCursor ();
			if (SceneManager.GetActiveScene().name == "Init")
				LoadGameScenes ();
			else if (GameCamera.Instance != null)
				StartCoroutine(OnGameSceneLoadedRoutine ());
		}

		IEnumerator OnGameSceneLoadedRoutine ()
		{
			gameModifierDict.Clear();
			foreach (GameModifier gameModifier in gameModifiers)
				gameModifierDict.Add(gameModifier.name, gameModifier);
			hideCursorTimer.onFinished += HideCursor;
			cursorMoveSpeed = cursorMoveSpeedPerScreenArea * Screen.width * Screen.height;
			screenEffectAnimator.Play("None");
			PauseMenu.Instance.Hide ();
			if (ArchivesManager.currentAccountIndex != -1)
			{
				AccountSelectMenu.Instance.gameObject.SetActive(false);
				PauseGame (false);
			}
			foreach (Canvas canvas in canvases)
				canvas.worldCamera = GameCamera.Instance.camera;
			if (onGameScenesLoaded != null)
			{
				onGameScenesLoaded ();
				onGameScenesLoaded = null;
			}
			yield return StartCoroutine(LoadRoutine ());
			yield break;
		}

		void Update ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Arrow.shotArrows.Clear();
				return;
			}
#endif
				if (!initialized)
					return;
			// try
			// {
				InputSystem.Update ();
				mousePosition = InputManager.GetMousePosition(MathfExtensions.NULL_INT);
				for (int i = 0; i < updatables.Length; i ++)
				{
					IUpdatable updatable = updatables[i];
					updatable.DoUpdate ();
				}
				Physics2D.Simulate(Time.deltaTime);
				ObjectPool.Instance.DoUpdate ();
				GameCamera.Instance.DoUpdate ();
				// GetSingleton<GraphicsManager>().DoUpdate ();
				HandleCursor ();
				HandlePausing ();
				framesSinceLoadedScene ++;
				previousMousePosition = mousePosition;
			// }
			// catch (Exception e)
			// {
			// 	print(e.Message + "\n" + e.StackTrace);
			// }
		}

#if UNITY_EDTIOR
		[UnityEditor.Callbacks.DidReloadScripts]
		static void OnScriptsReloaded ()
		{
			GameManager.updatables = FindObjectsOfType<IUpdatable>();
		}
#endif

		void InitCursor ()
		{
			cursorEntriesDict.Clear();
			foreach (CursorEntry cursorEntry in cursorEntries)
			{
				cursorEntriesDict.Add(cursorEntry.name, cursorEntry);
				cursorEntry.rectTrs.gameObject.SetActive(false);
			}
			Cursor.visible = false;
			activeCursorEntry = null;
			cursorEntriesDict["Default"].SetAsActive ();
		}

		void HandleCursor ()
		{
			// if (activeCursorEntry == null || activeCursorEntry.rectTrs == null)
			// 	InitCursor ();
			if (!InputManager.UsingGamepad)
			{
				activeCursorEntry.rectTrs.gameObject.SetActive(true);
				// activeCursorEntry.rectTrs.position = cursorCanvasRectTrs.sizeDelta.Multiply(Camera.main.ScreenToViewportPoint(mousePosition));
				activeCursorEntry.rectTrs.position = mousePosition;
			}
			else
			{
				if (mousePosition != previousMousePosition)
				{
					activeCursorEntry.rectTrs.gameObject.SetActive(true);
					hideCursorTimer.Reset ();
					hideCursorTimer.Start ();
					moveInput = InputManager.GetSwimInput(MathfExtensions.NULL_INT);// + InputManager.GetAxis2D("Aim Horizontal", "Aim Vertical");
					if (moveInput.magnitude > InputManager.Settings.defaultDeadzoneMin)
					{
						activeCursorEntry.rectTrs.position += (Vector3) moveInput * cursorMoveSpeed * Time.unscaledDeltaTime;
						activeCursorEntry.rectTrs.position = activeCursorEntry.rectTrs.position.ClampComponents(Vector3.zero, new Vector2(Screen.width, Screen.height));
					}
				}
				else if (WorldMap.isOpen)
				{
					activeCursorEntry.rectTrs.gameObject.SetActive(true);
					moveInput = InputManager.GetSwimInput(MathfExtensions.NULL_INT);// + InputManager.GetAxis2D("Aim Horizontal", "Aim Vertical");
					activeCursorEntry.rectTrs.position += (Vector3) moveInput * cursorMoveSpeed * Time.unscaledDeltaTime;
					activeCursorEntry.rectTrs.position = activeCursorEntry.rectTrs.position.ClampComponents(Vector3.zero, new Vector2(Screen.width, Screen.height));
				}
				else
				{
					if (hideCursorTimer.timerRoutine == null)
						activeCursorEntry.rectTrs.gameObject.SetActive(false);
					else
					{
						moveInput = InputManager.GetSwimInput(MathfExtensions.NULL_INT);// + InputManager.GetAxis2D("Aim Horizontal", "Aim Vertical");
						activeCursorEntry.rectTrs.position += (Vector3) moveInput * cursorMoveSpeed * Time.unscaledDeltaTime;
						activeCursorEntry.rectTrs.position = activeCursorEntry.rectTrs.position.ClampComponents(Vector3.zero, new Vector2(Screen.width, Screen.height));
					}
				}
			}
		}

		bool pauseInput;
		bool previousPauseInput;
		void HandlePausing ()
		{
			pauseInput = InputManager.GetPauseInput(MathfExtensions.NULL_INT);
			// Debug.Log(pauseInput);
			if (pauseInput && !previousPauseInput && ArchivesManager.currentAccountIndex != -1)
			{
				if (!PauseMenu.Instance.gameObject.activeSelf)
					PauseMenu.instance.Show ();
				else
					PauseMenu.instance.Hide ();
			}
			previousPauseInput = pauseInput;
		}

		public IEnumerator LoadRoutine ()
		{
			if (!HasPlayedBefore)
			{
				SaveAndLoadManager.Instance.DeleteAll ();
				HasPlayedBefore = true;
				SaveAndLoadManager.Instance.OnLoaded ();
			}
			else
				SaveAndLoadManager.Instance.LoadFromCurrentAccount ();
			if (screenBlockerGo != null)
				screenBlockerGo.SetActive(false);
			initialized = true;
			yield break;
		}

		public void HideCursor (params object[] args)
		{
			activeCursorEntry.rectTrs.gameObject.SetActive(false);
		}

		public void LoadScene (string name)
		{
			if (Instance != this)
			{
				instance.LoadScene (name);
				return;
			}
			framesSinceLoadedScene = 0;
			SceneManager.LoadScene(name);
		}

		public void LoadSceneAdditive (string name)
		{
			if (Instance != this)
			{
				instance.LoadSceneAdditive (name);
				return;
			}
			SceneManager.LoadScene(name, LoadSceneMode.Additive);
		}

		public void LoadScene (int index)
		{
			LoadScene (SceneManager.GetSceneByBuildIndex(index).name);
		}

		public void UnloadScene (string name)
		{
			AsyncOperation unloadGameScene = SceneManager.UnloadSceneAsync(name);
			unloadGameScene.completed += OnGameSceneUnloaded;
		}

		public void OnGameSceneUnloaded (AsyncOperation unloadGameScene)
		{
			unloadGameScene.completed -= OnGameSceneUnloaded;
		}

		public void ReloadActiveScene ()
		{
			LoadScene (SceneManager.GetActiveScene().name);
		}

		public void LoadGameScenes ()
		{
			if (Instance != this)
			{
				instance.LoadGameScenes ();
				return;
			}
			initialized = false;
			StopAllCoroutines ();
			if (SceneManager.GetSceneByName(gameScenes[0].name).isLoaded)
			{
				UnloadScene ("Game");
				LoadSceneAdditive ("Game");
				return;
			}
			LoadScene (gameScenes[0].name);
			GameScene gameScene;
			for (int i = 1; i < gameScenes.Length; i ++)
			{
				gameScene = gameScenes[i];
				if (gameScene.use)
					LoadSceneAdditive (gameScene.name);
			}
		}

		public void PauseGame (bool pause)
		{
			paused = pause;
			Time.timeScale = timeScale * (1 - paused.GetHashCode());
			AudioListener.pause = paused;
		}

		public void Quit ()
		{
			Application.Quit();
		}

		public void OnApplicationQuit ()
		{
			if (ArchivesManager.currentAccountIndex == -1)
				return;
#if UNITY_EDITOR
			Player.addToMoneyOnSave = 0;
#endif
			ArchivesManager.CurrentlyPlaying.PlayTime += Time.time;
			SaveAndLoadManager.Instance.SaveToCurrentAccount ();
			SaveAndLoadManager.ResetPersistantValues ();
		}

		public void OnApplicationFocus (bool isFocused)
		{
			GameManager.isFocused = isFocused;
			if (isFocused)
			{
				foreach (IUpdatable pausedUpdatable in pausedUpdatables)
					updatables = updatables.Add(pausedUpdatable);
				pausedUpdatables = new IUpdatable[0];
				foreach (Timer runningTimer in Timer.runningInstances)
					runningTimer.pauseIfCan = false;
				foreach (TemporaryActiveObject tempActiveObject in TemporaryActiveObject.activeInstances)
					tempActiveObject.Do ();
			}
			else
			{
				IUpdatable updatable;
				for (int i = 0; i < updatables.Length; i ++)
				{
					updatable = updatables[i];
					if (!updatable.PauseWhileUnfocused)
					{
						pausedUpdatables = pausedUpdatables.Add(updatable);
						updatables = updatables.RemoveAt(i);
						i --;
					}
				}
				foreach (Timer runningTimer in Timer.runningInstances)
					runningTimer.pauseIfCan = true;
				foreach (TemporaryActiveObject tempActiveObject in TemporaryActiveObject.activeInstances)
					tempActiveObject.Do ();
			}
		}

		public void SetGosActive ()
		{
			if (Instance != this)
			{
				instance.SetGosActive ();
				return;
			}
			string[] stringSeperators = { STRING_SEPERATOR };
			string[] enabledGos = enabledGosString.Split(stringSeperators, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < enabledGos.Length; i ++)
			{
				string goName = enabledGos[i];
				for (int i2 = 0; i2 < registeredGos.Length; i2 ++)
				{
					GameObject registeredGo = registeredGos[i2];
					if (goName == registeredGo.name)
					{
						registeredGo.SetActive(true);
						break;
					}
				}
			}
			string[] disabledGos = disabledGosString.Split(stringSeperators, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < disabledGos.Length; i ++)
			{
				string goName = disabledGos[i];
				GameObject go = GameObject.Find(goName);
				if (go != null)
					go.SetActive(false);
			}
		}
		
		public void ActivateGoForever (GameObject go)
		{
			go.SetActive(true);
			ActivateGoForever (go.name);
		}
		
		public void DeactivateGoForever (GameObject go)
		{
			go.SetActive(false);
			DeactivateGoForever (go.name);
		}
		
		public void ActivateGoForever (string goName)
		{
			disabledGosString = disabledGosString.Replace(STRING_SEPERATOR + goName, "");
			if (!enabledGosString.Contains(goName))
				enabledGosString += STRING_SEPERATOR + goName;
		}
		
		public void DeactivateGoForever (string goName)
		{
			enabledGosString = enabledGosString.Replace(STRING_SEPERATOR + goName, "");
			if (!disabledGosString.Contains(goName))
				disabledGosString += STRING_SEPERATOR + goName;
		}

		public void SetGameObjectActive (string name)
		{
			GameObject.Find(name).SetActive(true);
		}

		public void SetGameObjectInactive (string name)
		{
			GameObject.Find(name).SetActive(false);
		}

		public void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (Instance != this)
				return;
			if (WorldMap.Instance == null)
				updatables = new IUpdatable[0];
			else
				updatables = new IUpdatable[1] { WorldMap.instance };
			StopAllCoroutines();
			hideCursorTimer.onFinished -= HideCursor;
			// SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public void _LogError (string str)
		{
			LogError (str);
		}

		public static void LogError (object o)
		{
			Debug.LogError(o);
		}

		public void _Log (string str)
		{
			Log (str);
		}

		public static void Log (object obj)
		{
			print(obj);
		}

		public static Object Clone (Object obj)
		{
			return Instantiate(obj);
		}

		public static Object Clone (Object obj, Transform parent)
		{
			return Instantiate(obj, parent);
		}

		public static Object Clone (Object obj, Vector3 position, Quaternion rotation)
		{
			return Instantiate(obj, position, rotation);
		}

		public static void _Destroy (Object obj)
		{
			Destroy(obj);
		}

		public static void _DestroyImmediate (Object obj)
		{
			DestroyImmediate(obj);
		}

		public void ToggleGo (GameObject go)
		{
			go.SetActive(!go.activeSelf);
		}

		public void PressButton (Button button)
		{
			button.onClick.Invoke();
		}

		// public static T GetSingleton<T> ()
		// {
		// 	object obj = null;
		// 	if (!singletons.TryGetValue(typeof(T), out obj))
		// 		obj = GetSingleton<T>(FindObjectsOfType<Object>());
		// 	return (T) obj;
		// }

		// public static T GetSingleton<T> (Object[] objects)
		// {
		// 	if (typeof(T).IsSubclassOf(typeof(Object)))
		// 	{
		// 		for (int i = 0; i < objects.Length; i ++)
		// 		{
		// 			Object obj = objects[i];
		// 			if (obj is T)
		// 			{
		// 				if (singletons.ContainsKey(typeof(T)))
		// 					singletons[typeof(T)] = obj;
		// 				else
		// 					singletons.Add(typeof(T), obj);
		// 				return (T) (object) obj;
		// 			}
		// 		}
		// 	}
		// 	return (T) (object) null;
		// }

		public static bool ModifierIsActiveAndExists (string name)
		{
			GameModifier gameModifier;
			if (gameModifierDict.TryGetValue(name, out gameModifier))
				return gameModifier.isActive;
			else
				return false;
		}

		public static bool ModifierIsActive (string name)
		{
			return gameModifierDict[name].isActive;
		}

		public static bool ModifierExists (string name)
		{
			return gameModifierDict.ContainsKey(name);
		}

		[Serializable]
		public class CursorEntry
		{
			public string name;
			public RectTransform rectTrs;

			public void SetAsActive ()
			{
				if (activeCursorEntry != null)
					activeCursorEntry.rectTrs.gameObject.SetActive(false);
				rectTrs.gameObject.SetActive(true);
				activeCursorEntry = this;
			}
		}

		[Serializable]
		public class GameModifier
		{
			public string name;
			public bool isActive;
		}

		[Serializable]
		public class GameScene
		{
			public string name;
			public bool use = true;
		}
	}
}
