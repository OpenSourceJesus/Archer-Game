#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Extensions;
using UnityEditor.SceneManagement;
using System;
using DialogAndStory;

namespace ArcherGame
{
	public class WorldMakerWindow : EditorWindow
	{
		public static WorldMakerWindow instance;
		public static bool makeTilemaps;
		public static bool makeWorldObjects;
		public static bool enableWorld;
		public static bool enablePieces;
		public static bool piecesAreActive;
		public static bool worldIsActive;
		public static bool showPieces;
		public static bool piecesAreShown;
		public const float SMALL_DISTANCE = .1f;

		[MenuItem("Window/World")]
		public static void Init ()
		{
			instance = (WorldMakerWindow) EditorWindow.GetWindow(typeof(WorldMakerWindow));
			makeTilemaps = EditorPrefs.GetBool("Make tilemaps", true);
			makeWorldObjects = EditorPrefs.GetBool("Make objects", true);
			enableWorld = EditorPrefs.GetBool("Enable world", false);
			enablePieces = EditorPrefs.GetBool("Enable pieces", true);
			showPieces = EditorPrefs.GetBool("Show pieces", false);
			instance.Show();
		}

		public virtual void OnGUI ()
		{
			GUIContent guiContent = new GUIContent();
			guiContent.text = "Rebuild";
			bool rebuild = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (rebuild)
				Rebuild ();
			makeTilemaps = EditorGUILayout.Toggle("Make Tilemaps", makeTilemaps);
			EditorPrefsExtensions.SetBool("Make tilemaps", makeTilemaps);
			makeWorldObjects = EditorGUILayout.Toggle("Make Objects", makeWorldObjects);
			EditorPrefsExtensions.SetBool("Make objects", makeWorldObjects);
			guiContent.text = "Make Pieces";
			bool makePieces = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (makePieces)
				MakePieces ();
			guiContent.text = "Remove Pieces";
			bool removePieces = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (removePieces)
				RemovePieces ();
			enableWorld = EditorGUILayout.Toggle("Enable World", enableWorld);
			if (enableWorld != worldIsActive)
				SetWorldActive (enableWorld);
			EditorPrefsExtensions.SetBool("Enable world", enableWorld);
			enablePieces = EditorGUILayout.Toggle("Enable Pieces", enablePieces);
			if (enablePieces != piecesAreActive)
				SetPiecesActive (enablePieces);
			EditorPrefsExtensions.SetBool("Enable pieces", enablePieces);
			showPieces = EditorGUILayout.Toggle("Show Pieces", showPieces);
			if (showPieces != piecesAreShown)
				ShowPieces (showPieces);
			EditorPrefsExtensions.SetBool("Show pieces", showPieces);
			guiContent.text = "Use Enemy Battles Of World";
			bool useEnemyBattlesOfWorld = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (useEnemyBattlesOfWorld)
				UseEnemyBattlesOfWorld ();
			guiContent.text = "Use Enemy Battles Of Pieces";
			bool useEnemyBattlesOfPieces = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (useEnemyBattlesOfPieces)
				UseEnemyBattlesOfPieces ();
		}

		[MenuItem("World/Rebuild %&r")]
		public static void Rebuild ()
		{
			SetWorldActive (true);
			RemovePieces ();
			makeTilemaps = true;
			makeWorldObjects = true;
			MakePieces ();
			enableWorld = false;
			SetWorldActive (false);
			UseEnemyBattlesOfPieces ();
			enablePieces = true;
			SetPiecesActive (true);
			UpdateEnemyBattles ();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneByName("Game"));
		}

		public static void ShowPieces (bool show)
		{
			World.Instance.SetPieces ();
			foreach (WorldPiece worldPiece in World.Instance.pieces)
				worldPiece.gameObject.SetActive(show);
			piecesAreShown = show;
		}

		public static void SetWorldActive (bool active)
		{
			Transform sceneRoot = GameObject.Find("Game Scene").GetComponent<Transform>();
			for (int i = 0; i < sceneRoot.childCount; i ++)
			{
                for (int i2 = 0; i2 < World.Instance.tilemapsIncludedInPieces.Length; i2 ++)
                {
                    Tilemap tilemap = World.Instance.tilemapsIncludedInPieces[i2];
                    tilemap.gameObject.SetActive(active);
                }
                ObjectInWorld worldObject = sceneRoot.GetChild(i).GetComponent<ObjectInWorld>();
				if (worldObject != null && worldObject.enabled)
					worldObject.go.SetActive(active);
			}
			worldIsActive = active;
		}

		public static void SetPiecesActive (bool active)
		{
			World.Instance.piecesParent.gameObject.SetActive(active);
			piecesAreActive = active;
		}

		public static void UseEnemyBattlesOfWorld ()
		{
			for (int i = 0; i < World.Instance.enemyBattles.Length; i ++)
			{
				EnemyBattle enemyBattle = World.Instance.enemyBattles[i];
				for (int i2 = 0; i2 < enemyBattle.enemies.Length; i2 ++)
				{
					Enemy enemy = enemyBattle.enemies[i2];
					if (enemy.worldObject.IsInPieces)
					{
						ObjectInWorld enemyWorldObject = enemy.worldObject;
						if (enemyWorldObject.duplicateWorldObject != null)
						{
                            for (int i3 = 0; i3 < enemyWorldObject.duplicateWorldObject.gosWithComponents.Length; i3 ++)
							{
                                GameObject goWithComponents = enemyWorldObject.duplicateWorldObject.gosWithComponents[i3];
                                Enemy duplicateEnemy = goWithComponents.GetComponent<Enemy>();
								if (duplicateEnemy != null)
								{
									enemyBattle.enemies[i2] = duplicateEnemy;
									break;
								}
							}
						}
					}
				}
				for (int i2 = 0; i2 < enemyBattle.awakableEnemies.Length; i2 ++)
				{
					AwakableEnemy awakableEnemy = enemyBattle.awakableEnemies[i2];
					if (awakableEnemy.worldObject.IsInPieces)
					{
						ObjectInWorld awakableEnemyWorldObject = awakableEnemy.worldObject;
						if (awakableEnemyWorldObject.duplicateWorldObject != null)
						{
							for (int i3 = 0; i3 < awakableEnemyWorldObject.duplicateWorldObject.gosWithComponents.Length; i3 ++)
							{
								GameObject goWithComponents = awakableEnemyWorldObject.duplicateWorldObject.gosWithComponents[i3];
								AwakableEnemy duplicateAwakableEnemy = goWithComponents.GetComponent<AwakableEnemy>();
								if (duplicateAwakableEnemy != null)
								{
									enemyBattle.awakableEnemies[i2] = duplicateAwakableEnemy;
									break;
								}
							}
						}
					}
				}
			}
		}

		public static void UseEnemyBattlesOfPieces ()
		{
			for (int i = 0; i < World.Instance.enemyBattles.Length; i ++)
			{
				EnemyBattle enemyBattle = World.Instance.enemyBattles[i];
				for (int i2 = 0; i2 < enemyBattle.enemies.Length; i2 ++)
				{
					Enemy enemy = enemyBattle.enemies[i2];
					if (!enemy.worldObject.IsInPieces)
					{
						ObjectInWorld enemyWorldObject = enemy.worldObject;
						if (enemyWorldObject.duplicateWorldObject != null)
						{
                            for (int i3 = 0; i3 < enemyWorldObject.duplicateWorldObject.gosWithComponents.Length; i3 ++)
							{
                                GameObject goWithComponents = enemyWorldObject.duplicateWorldObject.gosWithComponents[i3];
                                Enemy duplicateEnemy = goWithComponents.GetComponent<Enemy>();
								if (duplicateEnemy != null)
								{
									enemyBattle.enemies[i2] = duplicateEnemy;
									break;
								}
							}
						}
					}
				}
				for (int i2 = 0; i2 < enemyBattle.awakableEnemies.Length; i2 ++)
				{
					AwakableEnemy awakableEnemy = enemyBattle.awakableEnemies[i2];
					if (!awakableEnemy.worldObject.IsInPieces)
					{
						ObjectInWorld awakableEnemyWorldObject = awakableEnemy.worldObject;
						if (awakableEnemyWorldObject.duplicateWorldObject != null)
						{
                            for (int i3 = 0; i3 < awakableEnemyWorldObject.duplicateWorldObject.gosWithComponents.Length; i3 ++)
							{
                                GameObject goWithComponents = awakableEnemyWorldObject.duplicateWorldObject.gosWithComponents[i3];
								AwakableEnemy duplicateAwakableEnemy = goWithComponents.GetComponent<AwakableEnemy>();
								if (duplicateAwakableEnemy != null)
								{
									enemyBattle.awakableEnemies[i2] = duplicateAwakableEnemy;
									break;
								}
							}
						}
					}
				}
				World.Instance.enemyBattles[i] = enemyBattle;
			}
		}

		[MenuItem("World/Update duplicate objects")]
		public static void UpdateDuplicateWorldObjects ()
		{
			ObjectInWorld[] worldObjects = FindObjectsOfType<ObjectInWorld>();
            for (int i = 0; i < worldObjects.Length; i ++)
			{
                ObjectInWorld worldObject = worldObjects[i];
                for (int i2 = 0; i2 < worldObjects.Length; i2 ++)
				{
                    ObjectInWorld otherWorldObject = worldObjects[i2];
                    if (i != i2 && (worldObject.trs.position - otherWorldObject.trs.position).sqrMagnitude <= SMALL_DISTANCE)
					{
						worldObject.duplicateWorldObject = otherWorldObject;
						worldObject.duplicateTrs = otherWorldObject.trs;
						worldObject.duplicateGo = otherWorldObject.go;
						otherWorldObject.duplicateWorldObject = worldObject;
						otherWorldObject.duplicateTrs = worldObject.trs;
						otherWorldObject.duplicateGo = worldObject.go;
					}
				}
			}
		}

		[MenuItem("World/Update enemy battles")]
		public static void UpdateEnemyBattles ()
		{
			World.Instance.enemyBattles = FindObjectsOfType<EnemyBattle>();
            for (int i = 0; i < World.Instance.enemyBattles.Length; i ++)
			{
                EnemyBattle enemyBattle = World.Instance.enemyBattles[i];
                for (int i2 = 0; i2 < enemyBattle.enemies.Length; i2 ++)
				{
                    Enemy enemy = enemyBattle.enemies[i2];
                    enemy.battleIAmPartOf = enemyBattle;
					enemy.Init ();
				}
			}
		}
		
		[MenuItem("World/Make pieces")]
		public static void MakePieces ()
		{
			WorldPiece piece;
			TileBase[] tileBases;
			ObjectInWorld originalWorldObject;
			ObjectInWorld newWorldObject;
			Vector2Int pieceLocation = new Vector2Int();
			Vector2Int cellBoundsMin;
			Vector2Int cellBoundsMax;
			Vector2 worldBoundsMin;
			Vector2 worldBoundsMax;
			List<ObjectInWorld> worldObjects = new List<ObjectInWorld>();
			worldObjects.AddRange(World.Instance.worldObjects);
			for (int x = World.Instance.cellBoundsRect.xMin; x < World.Instance.cellBoundsRect.xMax; x += World.Instance.sizeOfPieces.x)
			{
				pieceLocation.y = 0;
				for (int y = World.Instance.cellBoundsRect.yMin; y < World.Instance.cellBoundsRect.yMax; y += World.Instance.sizeOfPieces.y)
				{
					piece = (WorldPiece) PrefabUtility.InstantiatePrefab(World.Instance.piecePrefab);
					piece.trs.SetParent(World.Instance.piecesParent);
					piece.location = pieceLocation;
					piece.name += "[" + pieceLocation + "]";
					cellBoundsMin = new Vector2Int(x, y);
					cellBoundsMax = new Vector2Int(Mathf.Clamp(x + World.Instance.sizeOfPieces.x, World.Instance.cellBoundsRect.xMin, World.Instance.cellBoundsRect.xMax), Mathf.Clamp(y + World.Instance.sizeOfPieces.y, World.Instance.cellBoundsRect.yMin, World.Instance.cellBoundsRect.yMax));
					piece.cellBoundsRect = new RectInt();
					piece.cellBoundsRect.SetMinMax(cellBoundsMin, cellBoundsMax);
					worldBoundsMin = World.Instance.tilemapsIncludedInPieces[0].GetCellCenterWorld(cellBoundsMin.ToVec3Int()) - (World.Instance.tilemapsIncludedInPieces[0].cellSize / 2);
					worldBoundsMax = World.Instance.tilemapsIncludedInPieces[0].GetCellCenterWorld(cellBoundsMax.ToVec3Int()) + (World.Instance.tilemapsIncludedInPieces[0].cellSize / 2);
					piece.worldBoundsRect = Rect.MinMaxRect(worldBoundsMin.x, worldBoundsMin.y, worldBoundsMax.x, worldBoundsMax.y);
					if (makeTilemaps)
					{
						piece.tilemaps = new Tilemap[World.Instance.tilemapsIncludedInPieces.Length];
						for (int i = 0; i < piece.tilemaps.Length; i ++)
						{
							GameObject newTilemapGo = (GameObject) PrefabUtility.InstantiatePrefab(GameManager.Instance.emptyGoPrefab);
							Tilemap worldTilemap = World.Instance.tilemapsIncludedInPieces[i];
							GameObject worldTilemapGo = worldTilemap.gameObject;
							List<AddedGameObject> addedGos = PrefabUtility.GetAddedGameObjects(worldTilemapGo);
							List<AddedComponent> addedComponents = PrefabUtility.GetAddedComponents(worldTilemapGo);
							PropertyModification[] propertyModifications = PrefabUtility.GetPropertyModifications(worldTilemapGo);
							Transform worldTilemapTrs = worldTilemapGo.GetComponent<Transform>();
							Transform newTilemapTrs = newTilemapGo.GetComponent<Transform>();
							Transform addedTrs;
                            for (int i2 = 0; i2 < addedGos.Count; i2 ++)
							{
                                AddedGameObject addedGo = addedGos[i2];
                                addedTrs = addedGo.instanceGameObject.GetComponent<Transform>();
								GameObject addedGoClone = (GameObject) GameManager.Clone (addedGo.instanceGameObject, TransformExtensions.FindEquivalentChild(worldTilemapTrs, addedTrs, newTilemapTrs));
								addedGoClone.GetComponent<Transform>().localScale = addedTrs.localScale;
							}
							piece.tilemaps[i] = newTilemapGo.AddComponent<Tilemap>();
							BoundsInt pieceBounds = piece.cellBoundsRect.Move(-piece.cellBoundsRect.size / 2).ToBoundsInt();
							pieceBounds.size = pieceBounds.size.SetZ(1);
							tileBases = worldTilemap.GetTilesBlock(pieceBounds);
							piece.tilemaps[i].SetTilesBlock(pieceBounds, tileBases);
							foreach (Vector3Int cellPositionInPiece in pieceBounds.allPositionsWithin)
								piece.tilemaps[i].SetTransformMatrix(cellPositionInPiece, worldTilemap.GetTransformMatrix(cellPositionInPiece));
							piece.tilemaps[i].color = worldTilemap.color;
                            for (int i2 = 0; i2 < addedComponents.Count; i2 ++)
							{
                                AddedComponent addedComponent = addedComponents[i2];
                                if (addedComponent.instanceComponent.GetType() != typeof(Tilemap))
								{
									var newComponent = newTilemapGo.AddComponent(addedComponent.instanceComponent.GetType());
									ICopyable copyable = newComponent as ICopyable;
									if (copyable != null)
										copyable.Copy (worldTilemapGo.GetComponent(addedComponent.instanceComponent.GetType()));
								}
							}
							TilemapRenderer worldTilemapRenderer = worldTilemapGo.GetComponent<TilemapRenderer>();
							TilemapRenderer newTilemapRenderer = newTilemapGo.GetComponent<TilemapRenderer>();
							newTilemapRenderer.sortingOrder = worldTilemapRenderer.sortingOrder;
							newTilemapRenderer.sortingLayerName = worldTilemapRenderer.sortingLayerName;
							PropertyModification propertyModification;
							for (int i2 = 0; i2 < propertyModifications.Length; i2 ++)
							{
								propertyModification = propertyModifications[i2];
								if (propertyModification.objectReference == null || propertyModification.target == null)
								{
									propertyModifications = propertyModifications.RemoveAt(i2);
									i2 --;
								}
							}
							if (propertyModifications.Length > 0)
								PrefabUtility.SetPropertyModifications(newTilemapGo, propertyModifications);
							// foreach (MonoBehaviour dontPreserveScript in World.Instance.dontPreserveScripts)
							// {
							//     if (piece.tilemaps[i].GetComponent(dontPreserveScript.name) != null)
							//         DestroyImmediate(piece.tilemaps[i].GetComponent(dontPreserveScript.name));
							// }
							newTilemapTrs.SetParent(piece.tilemapsParent);
							newTilemapTrs.localPosition = Vector2.zero;
							newTilemapTrs.name = worldTilemap.name;
							newTilemapGo.layer = worldTilemapGo.layer;
						}
					}
					if (makeWorldObjects)
					{
						for (int i = 0; i < worldObjects.Count; i ++)
						{
							originalWorldObject = worldObjects[i];
							if (piece.worldBoundsRect.Contains(originalWorldObject.trs.position.ToVec2Int()))
							{
								// if (PrefabUtility.GetPrefabInstanceStatus(originalWorldObject) == PrefabInstanceStatus.NotAPrefab)
									newWorldObject = Instantiate(originalWorldObject);
								// else
								// 	newWorldObject = PrefabUtilityExtensions.ClonePrefabInstance(originalWorldObject.gameObject).GetComponent<ObjectInWorld>();
								newWorldObject.trs.SetParent(piece.worldObjectsParent);
								newWorldObject.trs.position = originalWorldObject.trs.position;
								newWorldObject.trs.rotation = originalWorldObject.trs.rotation;
								newWorldObject.trs.localScale = originalWorldObject.trs.localScale;
								newWorldObject.duplicateTrs = originalWorldObject.trs;
								newWorldObject.duplicateGo = originalWorldObject.go;
								newWorldObject.duplicateWorldObject = originalWorldObject;
								newWorldObject.pieceIAmIn = piece;
								originalWorldObject.duplicateTrs = newWorldObject.trs;
								originalWorldObject.duplicateGo = newWorldObject.go;
								originalWorldObject.duplicateWorldObject = newWorldObject;
								// foreach (MonoBehaviour dontPreserveScript in World.Instance.dontPreserveScripts)
								// {
								//     if (newWorldObject.GetComponent(dontPreserveScript.name) != null)
								//         DestroyImmediate(newWorldObject.GetComponent(dontPreserveScript.name));
								// }
								newWorldObject.name = originalWorldObject.name;
								newWorldObject.enabled = false;
								List<ICopyable> originalCopyables = new List<ICopyable>();
                                for (int i2 = 0; i2 < originalWorldObject.gosWithComponents.Length; i2 ++)
                                {
                                    GameObject goWithComponents = originalWorldObject.gosWithComponents[i2];
                                    originalCopyables.AddRange(goWithComponents.GetComponents<ICopyable>());
                                }
                                List<ICopyable> newCopyables = new List<ICopyable>();
                                for (int i2 = 0; i2 < newWorldObject.gosWithComponents.Length; i2 ++)
								{
                                    GameObject goWithComponents = newWorldObject.gosWithComponents[i2];
                                    Enemy enemy = goWithComponents.GetComponent<Enemy>();
									if (enemy != null)
										enemy.worldObject = newWorldObject;
									newCopyables.AddRange(goWithComponents.GetComponents<ICopyable>());
								}
								for (int i2 = 0; i2 < newCopyables.Count; i2 ++)
									newCopyables[i2].Copy (originalCopyables[i2]);
								worldObjects.RemoveAt(i);
								i --;
							}
						}
					}
					piece.gameObject.SetActive(false);
					pieceLocation.y ++;
				}
				pieceLocation.x ++;
			}
			ObjectInWorld firstEnemyWorldObject;
			ObjectInWorld otherEnemyWorldObject;
            for (int i = 0; i < World.Instance.enemyBattles.Length; i ++)
			{
                EnemyBattle enemyBattle = World.Instance.enemyBattles[i];
                firstEnemyWorldObject = enemyBattle.enemies[0].worldObject;
				if (!firstEnemyWorldObject.IsInPieces)
					firstEnemyWorldObject = firstEnemyWorldObject.duplicateWorldObject;
				for (int i2 = 1; i2 < enemyBattle.enemies.Length; i2 ++)
				{
					otherEnemyWorldObject = enemyBattle.enemies[i2].worldObject;
					if (!otherEnemyWorldObject.IsInPieces)
						otherEnemyWorldObject = otherEnemyWorldObject.duplicateWorldObject;
					otherEnemyWorldObject.trs.SetParent(firstEnemyWorldObject.trs.parent);
					otherEnemyWorldObject.pieceIAmIn = firstEnemyWorldObject.pieceIAmIn;
					firstEnemyWorldObject.pieceIAmIn.worldBoundsRect = firstEnemyWorldObject.pieceIAmIn.worldBoundsRect.GrowToPoint(otherEnemyWorldObject.trs.position);
				}
			}
			ObjectInWorld worldObject;
            for (int i = 0; i < World.Instance.moveTiles.Length; i ++)
			{
                MoveTile moveTile = World.Instance.moveTiles[i];
                worldObject = moveTile.worldObject;
				if (!moveTile.worldObject.IsInPieces)
					worldObject = worldObject.duplicateWorldObject;
                for (int i2 = 0; i2 < moveTile.wayPoints.Length; i2++)
                {
                    Transform wayPoint = moveTile.wayPoints[i2];
                    worldObject.pieceIAmIn.worldBoundsRect = worldObject.pieceIAmIn.worldBoundsRect.GrowToPoint(wayPoint.position);
                }
            }
            for (int i = 0; i < QuestManager.Instance.quests.Length; i ++)
			{
                Quest quest = QuestManager.Instance.quests[i];
                KillingQuest killingQuest = quest as KillingQuest;
				if (killingQuest != null)
				{
					for (int i2 = 0; i2 < killingQuest.enemiesToKill.Length; i2 ++)
					{
						worldObject = killingQuest.enemiesToKill[i2].worldObject;
						if (!worldObject.IsInPieces)
							worldObject = worldObject.duplicateWorldObject;
						for (int i3 = 0; i3 < worldObject.gosWithComponents.Length; i3 ++)
						{
							GameObject goWithComponents = worldObject.gosWithComponents[i3];
							Enemy enemy = goWithComponents.GetComponent<Enemy>();
							if (enemy != null)
							{
								killingQuest.enemiesToKill[i2] = enemy;
								break;
							}
						}
					}
				}
			}
			World.Instance.maxPieceLocation = pieceLocation - Vector2Int.one;
		}

		[MenuItem("World/Remove pieces")]
		public static void RemovePieces ()
		{
			UseEnemyBattlesOfWorld ();
            for (int i = 0; i < QuestManager.Instance.quests.Length; i ++)
			{
                Quest quest = QuestManager.Instance.quests[i];
                KillingQuest killingQuest = quest as KillingQuest;
				if (killingQuest != null)
				{
					for (int i2 = 0; i2 < killingQuest.enemiesToKill.Length; i2 ++)
					{
						ObjectInWorld enemyWorldObject = killingQuest.enemiesToKill[i2].worldObject;
						if (enemyWorldObject.IsInPieces)
							killingQuest.enemiesToKill[i2] = enemyWorldObject.duplicateGo.GetComponent<Enemy>();
					}
				}
			}
			for (int i = 0; i < World.Instance.piecesParent.childCount; i ++)
			{
				WorldPiece piece = World.Instance.piecesParent.GetChild(i).GetComponent<WorldPiece>();
				DestroyImmediate(piece.gameObject);
				i --;
			}
		}
		
		[MenuItem("World/Select World %&w")]
		public static void SelectWorld ()
		{
			Selection.activeTransform = World.Instance.trs;
		}
	}
}
#endif