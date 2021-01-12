using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DialogAndStory;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class WorldMap : SaveAndLoadObject, ISaveableAndLoadable, IUpdatable
	{
		public static WorldMap instance;
		public static WorldMap Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<WorldMap>();
				return instance;
			}
		}
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Tilemap[] tilemaps = new Tilemap[0];
		public Tilemap unexploredTilemap;
		[SaveAndLoadValue(false)]
		public static HashSet<Vector2Int> exploredCellPositions = new HashSet<Vector2Int>();
		public static HashSet<Vector2Int> exploredCellPositionsAtLastTimeOpened = new HashSet<Vector2Int>();
		Vector2Int cellPosition;
		// [HideInInspector]
		// public Vector2Int minExploredCellPosition;
		// [HideInInspector]
		// public Vector2Int maxExploredCellPosition;
		[HideInInspector]
		public Vector2Int minCellPosition;
		[HideInInspector]
		public Vector2Int maxCellPosition;
		Vector2Int previousMinCellPosition;
		public static List<WorldMapIcon> worldMapIcons = new List<WorldMapIcon>();
		public static bool isOpen;
		bool canControlCamera;
		public float cameraMoveSpeed;
		Vector2 moveInput;
		public float normalizedScreenBorder;
		Rect screenWithoutBorder;
		Obelisk fastTravelToObelisk;
		public TileBase unexploredTile;
		public WorldMapCamera worldMapCamera;
		[SaveAndLoadValue(false)]
		public static HashSet<Vector2Int> foundWorldPiecesLocations = new HashSet<Vector2Int>();
#if UNITY_EDITOR
		public bool update;
		public bool startOver;
		public int x;
		public int y;
#endif

		public void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorApplication.update -= DoEditorUpdate;
				return;
			}
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

#if UNITY_EDITOR
		public void DoEditorUpdate ()
		{
			if (!update)
				return;
			if (startOver)
			{
				World.Instance.update = true;
				World.instance.DoEditorUpdate ();
				x = World.Instance.cellBoundsRect.xMin;
				y = World.Instance.cellBoundsRect.yMin;
				startOver = false;
			}
			unexploredTilemap.SetTile(new Vector3Int(x, y, 0), unexploredTile);
			if (x > World.Instance.cellBoundsRect.xMax + 1)
			{
				x = World.instance.cellBoundsRect.xMin;
				y ++;
				if (y > World.instance.cellBoundsRect.yMax + 1)
					update = false;
			}
			else
				x ++;
		}
#endif

		public void Init ()
		{
			if (!enabled)
				return;
			minCellPosition = unexploredTilemap.WorldToCell(GameCamera.Instance.viewRect.min).ToVec2Int();
			maxCellPosition = unexploredTilemap.WorldToCell(GameCamera.Instance.viewRect.max).ToVec2Int();
			if (exploredCellPositions.Count == 0)
			{
				for (int x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					for (int y = minCellPosition.y; y <= maxCellPosition.y; y ++)
						exploredCellPositions.Add(new Vector2Int(x, y));
				}
				// minExploredCellPosition = minCellPosition;
				// maxExploredCellPosition = maxCellPosition;
			}
			// else
			// {
			// 	foreach (Vector2Int exploredCellPosition in exploredCellPositions)
			// 	{
			// 		// minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, exploredCellPosition);
			// 		// maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, exploredCellPosition);
			// 	}
			// }
			previousMinCellPosition = minCellPosition;
			screenWithoutBorder = new Rect();
			screenWithoutBorder.size = new Vector2(Screen.width - normalizedScreenBorder * Screen.width, Screen.height - normalizedScreenBorder * Screen.height);
			screenWithoutBorder.center = new Vector2(Screen.width / 2, Screen.height / 2);
			worldMapCamera.HandleViewSize ();
		}
		
		public void DoUpdate ()
		{
			if (!isOpen)
				UpdateExplored ();
			else
			{
				worldMapCamera.DoUpdate ();
				moveInput = InputManager.GetSwimInput(MathfExtensions.NULL_INT)+ InputManager.GetAimInput(MathfExtensions.NULL_INT);
				GameManager.activeCursorEntry.rectTrs.gameObject.SetActive(true);
				if (InputManager.UsingGamepad)
				{
					GameManager.activeCursorEntry.rectTrs.position += (Vector3) moveInput * GameManager.cursorMoveSpeed * Time.unscaledDeltaTime;
					GameManager.activeCursorEntry.rectTrs.position = GameManager.activeCursorEntry.rectTrs.position.ClampComponents(Vector3.zero, new Vector2(Screen.width, Screen.height));
				}
				if (!screenWithoutBorder.Contains(GameManager.activeCursorEntry.rectTrs.position))
				{
					moveInput = (Vector2) GameManager.activeCursorEntry.rectTrs.position - new Vector2(Screen.width / 2, Screen.height / 2);
					moveInput /= new Vector2(Screen.width / 2, Screen.height / 2).magnitude;
					if (canControlCamera)
						worldMapCamera.trs.position += (Vector3) moveInput * cameraMoveSpeed * Time.unscaledDeltaTime;
					if (GameManager.activeCursorEntry.name != "Arrow")
					{
						GameManager.cursorEntriesDict["Arrow"].SetAsActive ();
						GameManager.activeCursorEntry.rectTrs.position = GameManager.cursorEntriesDict["Default"].rectTrs.position;
					}
					GameManager.activeCursorEntry.rectTrs.up = moveInput;
					GameManager.Instance.worldMapMoveViewTutorialDialog.gameObject.SetActive(false);
				}
				else if (GameManager.activeCursorEntry.name != "Default")
				{
					GameManager.cursorEntriesDict["Default"].SetAsActive ();
					GameManager.activeCursorEntry.rectTrs.position = GameManager.cursorEntriesDict["Arrow"].rectTrs.position;
				}
				if (Obelisk.playerIsAtObelisk)
					HandleFastTravel ();
			}
		}

		bool interactInput;
		bool previousInteractInput;
		public void HandleFastTravel ()
		{
			interactInput = InputManager.GetInteractInput(MathfExtensions.NULL_INT);
			foreach (Obelisk obelisk in Obelisk.instances)
			{
				if (obelisk.found)
				{
					if (fastTravelToObelisk != obelisk && obelisk.worldMapIcon.collider.bounds.ToRect().Contains(worldMapCamera.camera.ScreenToWorldPoint(GameManager.activeCursorEntry.rectTrs.position)))
					{
						if (fastTravelToObelisk != null)
							fastTravelToObelisk.worldMapIcon.Unhighlight ();
						fastTravelToObelisk = obelisk;
						fastTravelToObelisk.worldMapIcon.Highlight ();
						break;
					}
				}
			}
			if (fastTravelToObelisk != null && interactInput && !previousInteractInput)
			{
				Obelisk.playerJustFastTraveled = true;
				Player.instance.spawnPosition = fastTravelToObelisk.worldMapIcon.collider.bounds.center;
				// SaveAndLoadManager.SaveNonSharedData ();
				SaveAndLoadManager.Instance.SaveToCurrentAccount ();
				Close ();
				GameManager.Instance.LoadGameScenes ();
			}
			previousInteractInput = interactInput;
		}

		public void Open ()
		{
			if (WorldMap.Instance != this)
			{
				WorldMap.instance.Open ();
				return;
			}
			if (isOpen)
				return;
			canControlCamera = false;
			isOpen = true;
			GameManager.Instance.PauseGame (true);
			for (int i = 0; i < worldMapIcons.Count; i ++)
			{
				WorldMapIcon worldMapIcon = worldMapIcons[i];
				foreach (Vector2Int position in worldMapIcon.cellBoundsRect.allPositionsWithin)
				{
					if (!worldMapIcon.onlyMakeIfExplored || exploredCellPositions.Contains(position))
						worldMapIcon.MakeIcon ();
				}
			}
			unexploredTilemap.gameObject.SetActive(true);
			HashSet<Vector2Int> exploredCellPositionsSinceLastTimeOpened = new HashSet<Vector2Int>();
			foreach (Vector2Int exploredCellPosition in exploredCellPositions)
				exploredCellPositionsSinceLastTimeOpened.Add(exploredCellPosition);
			foreach (Vector2Int exploredCellPositionAtLastTimeOpened in exploredCellPositionsAtLastTimeOpened)
				exploredCellPositionsSinceLastTimeOpened.Remove(exploredCellPositionAtLastTimeOpened);
			for (int i = 0; i < tilemaps.Length; i ++)
			{
				Tilemap worldMapTilemap = tilemaps[i];
				worldMapTilemap.gameObject.SetActive(true);
			}
			foreach (Vector2Int exploredCellPositionSinceLastTimeOpened in exploredCellPositionsSinceLastTimeOpened)
			{
				unexploredTilemap.SetTile(exploredCellPositionSinceLastTimeOpened.ToVec3Int(), null);
				for (int i = 0; i < tilemaps.Length; i ++)
				{
					Tilemap worldTilemap = World.Instance.tilemaps[i];
					Tilemap worldMapTilemap = tilemaps[i];
					TileBase tile = worldTilemap.GetTile(exploredCellPositionSinceLastTimeOpened.ToVec3Int());
					if (tile != null)
					{
						worldMapTilemap.SetTile(exploredCellPositionSinceLastTimeOpened.ToVec3Int(), tile);
						worldMapTilemap.SetTransformMatrix(exploredCellPositionSinceLastTimeOpened.ToVec3Int(), worldTilemap.GetTransformMatrix(exploredCellPositionSinceLastTimeOpened.ToVec3Int()));
					}
				}
			}
			worldMapCamera.trs.position = Player.instance.trs.position.SetZ(worldMapCamera.trs.position.z);
			worldMapCamera.gameObject.SetActive(true);
			if (GameManager.Instance.worldMapTutorialConversation.gameObject.activeSelf && GameManager.instance.worldMapTutorialConversation.updateRoutine == null)
			{
				DialogManager.Instance.StartConversation (GameManager.Instance.worldMapTutorialConversation);
				for (int i = 0; i < GameManager.Instance.worldMapTutorialConversation.dialogs.Length; i ++)
				{
					Dialog dialog = GameManager.Instance.worldMapTutorialConversation.dialogs[i];
					dialog.canvas.worldCamera = worldMapCamera.camera;
				}
			}
			if (InputManager.UsingGamepad)
			{
				GameManager.cursorEntriesDict["Default"].SetAsActive ();
				GameManager.activeCursorEntry.rectTrs.localPosition = Vector2.zero;
			}
			foreach (Vector2Int foundWorldPieceLocation in foundWorldPiecesLocations)
			{
				WorldPiece foundWorldPiece = World.instance.pieces[foundWorldPieceLocation.x, foundWorldPieceLocation.y];
				foundWorldPiece.gameObject.SetActive(true);
				Enemy[] enemies = foundWorldPiece.worldObjectsParent.GetComponentsInChildren<Enemy>();
				for (int i = 0; i < enemies.Length; i ++)
				{
					Enemy enemy = enemies[i];
					if (enemy.spriteSkin != null)
						enemy.spriteSkin.enabled = true;
				}
			}
			StopAllCoroutines();
			StartCoroutine(OpenRoutine ());
		}

		public IEnumerator OpenRoutine ()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			canControlCamera = true;
		}

		public void Close ()
		{
			if (WorldMap.Instance != this)
			{
				WorldMap.instance.Close ();
				return;
			}
			if (!isOpen)
				return;
			isOpen = false;
			exploredCellPositionsAtLastTimeOpened.Clear();
			foreach (Vector2Int exploredCellPosition in exploredCellPositions)
				exploredCellPositionsAtLastTimeOpened.Add(exploredCellPosition);
			for (int i = 0; i < worldMapIcons.Count; i ++)
			{
				WorldMapIcon worldMapIcon = worldMapIcons[i];
				worldMapIcon.DestroyIcon ();
				worldMapIcon.Unhighlight ();
			}
			for (int i = 0; i < tilemaps.Length; i ++)
			{
				Tilemap worldMapTilemap = tilemaps[i];
				worldMapTilemap.gameObject.SetActive(false);
			}
			worldMapCamera.gameObject.SetActive(false);
			if (!PauseMenu.Instance.gameObject.activeSelf)
				GameManager.Instance.PauseGame (false);
			if (!InputManager.UsingGamepad)
				GameManager.cursorEntriesDict["Default"].SetAsActive ();
			else
				GameManager.activeCursorEntry.rectTrs.gameObject.SetActive(false);
			HashSet<Vector2Int> disableWorldPieces = new HashSet<Vector2Int>(foundWorldPiecesLocations);
			for (int i = 0; i < World.instance.activePieces.Count; i ++)
			{
				WorldPiece activeWorldPiece = World.instance.activePieces[i];
				disableWorldPieces.Remove(activeWorldPiece.location);
			}
			foreach (Vector2Int disableWorldPieceLocation in disableWorldPieces)
			{
				WorldPiece disableWorldPiece = World.instance.pieces[disableWorldPieceLocation.x, disableWorldPieceLocation.y];
				disableWorldPiece.gameObject.SetActive(false);
				Enemy[] enemies = disableWorldPiece.worldObjectsParent.GetComponentsInChildren<Enemy>();
				for (int i = 0; i < enemies.Length; i ++)
				{
					Enemy enemy = enemies[i];
					if (enemy.spriteSkin != null)
						enemy.spriteSkin.enabled = false;
				}
			}
		}
		
		public void UpdateExplored ()
		{
			int x;
			int y;
			minCellPosition = unexploredTilemap.WorldToCell(GameCamera.Instance.viewRect.min).ToVec2Int();
			maxCellPosition = unexploredTilemap.WorldToCell(GameCamera.Instance.viewRect.max).ToVec2Int();
			if (TeleportArrow.justTeleported)
			{
				TeleportArrow.justTeleported = false;
				// minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, minCellPosition);
				// maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, maxCellPosition);
				for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				previousMinCellPosition = minCellPosition;
				return;
			}
			if (minCellPosition.x > previousMinCellPosition.x)
			{
				x = maxCellPosition.x;
				// if (maxCellPosition.x > maxExploredCellPosition.x)
				// 	maxExploredCellPosition.x = maxCellPosition.x;
				for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.y > previousMinCellPosition.y)
				{
					y = maxCellPosition.y;
					// if (maxCellPosition.y > maxExploredCellPosition.y)
					// 	maxExploredCellPosition.y = maxCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.y < previousMinCellPosition.y)
				{
					y = minCellPosition.y;
					// if (minCellPosition.y < minExploredCellPosition.y)
					// 	minExploredCellPosition.y = minCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			else if (minCellPosition.x < previousMinCellPosition.x)
			{
				x = minCellPosition.x;
				// if (minCellPosition.x < minExploredCellPosition.x)
				// 	minExploredCellPosition.x = minCellPosition.x;
				for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.y > previousMinCellPosition.y)
				{
					y = maxCellPosition.y;
					// if (maxCellPosition.y > maxExploredCellPosition.y)
					// 	maxExploredCellPosition.y = maxCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.y < previousMinCellPosition.y)
				{
					y = minCellPosition.y;
					// if (minCellPosition.y < minExploredCellPosition.y)
					// 	minExploredCellPosition.y = minCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			else if (minCellPosition.y > previousMinCellPosition.y)
			{
				y = maxCellPosition.y;
				// if (maxCellPosition.y > maxExploredCellPosition.y)
				// 	maxExploredCellPosition.y = maxCellPosition.y;
				for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.x > previousMinCellPosition.x)
				{
					x = maxCellPosition.x;
					// if (maxCellPosition.x > maxExploredCellPosition.x)
					// 	maxExploredCellPosition.x = maxCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.x < previousMinCellPosition.x)
				{
					x = minCellPosition.x;
					// if (minCellPosition.x < minExploredCellPosition.x)
					// 	minExploredCellPosition.x = minCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			else if (minCellPosition.y < previousMinCellPosition.y)
			{
				y = minCellPosition.y;
				// if (minCellPosition.y < minExploredCellPosition.y)
				// 	minExploredCellPosition.y = minCellPosition.y;
				for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.x > previousMinCellPosition.x)
				{
					x = maxCellPosition.x;
					// if (maxCellPosition.x > maxExploredCellPosition.x)
					// 	maxExploredCellPosition.x = maxCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.x < previousMinCellPosition.x)
				{
					x = minCellPosition.x;
					// if (minCellPosition.x < minExploredCellPosition.x)
					// 	minExploredCellPosition.x = minCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			previousMinCellPosition = minCellPosition;
		}
	}
}