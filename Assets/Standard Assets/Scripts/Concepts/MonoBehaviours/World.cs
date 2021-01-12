using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	[ExecuteInEditMode]
	public class World : SingletonMonoBehaviour<World>, IUpdatable
	{
		public Transform trs;
		public Tilemap[] tilemaps;
		public Tilemap[] tilemapsIncludedInPieces;
		public ObjectInWorld[] worldObjects;
		public MoveTile[] moveTiles;
		public Vector2Int sizeOfPieces;
		public WorldPiece piecePrefab;
		// public Dictionary<Vector2Int, WorldPiece> piecesDict = new Dictionary<Vector2Int, WorldPiece>();
		public WorldPiece[,] pieces;
		public Vector2Int maxPieceLocation;
		public Transform piecesParent;
		public RectInt cellBoundsRect;
		public Rect worldBoundsRect;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Vector2Int loadPiecesRange;
		public List<WorldPiece> piecesNearPlayer = new List<WorldPiece>();
		public List<WorldPiece> activePieces = new List<WorldPiece>();
		WorldPiece[] surroundingPieces;
		Rect loadPieceRangeRect = new Rect();
		// public MonoBehaviour[] dontPreserveScripts = new MonoBehaviour[0];
		public EnemyBattle[] enemyBattles = new EnemyBattle[0];
#if UNITY_EDITOR
		public EnemyBattle enemyBattlePrefab;
		public bool update;
#endif

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
			{
				EditorApplication.update -= DoEditorUpdate;
				WorldMakerWindow.SetWorldActive (false);
				WorldMakerWindow.SetPiecesActive (true);
				WorldMakerWindow.ShowPieces (false);
			}
#endif
			GameManager.updatables = GameManager.updatables.Add(this);
		}

#if UNITY_EDITOR
		public virtual void DoEditorUpdate ()
		{
			if (!update)
				return;
			update = false;
			worldObjects = FindObjectsOfType<ObjectInWorld>();
			ObjectInWorld worldObject;
			for (int i = 0; i < worldObjects.Length; i ++)
			{
				worldObject = worldObjects[i];
				if (!worldObject.enabled || worldObject.trs.parent.GetComponent<ObjectInWorld>() != null)
				{
					worldObjects = worldObjects.RemoveAt(i);
					i --;
				}
			}
			moveTiles = FindObjectsOfType<MoveTile>();
			cellBoundsRect = new RectInt();
			cellBoundsRect.min = Vector2Int.zero;
			cellBoundsRect.max = Vector2Int.zero;
			for (int i = 0; i < tilemaps.Length; i ++)
			{
				Tilemap tilemap = tilemaps[i];
				cellBoundsRect.SetMinMax(cellBoundsRect.min.SetToMinComponents(tilemap.cellBounds.min.ToVec2Int()), cellBoundsRect.max.SetToMaxComponents(tilemap.cellBounds.max.ToVec2Int()));
			}
			Vector2 worldBoundsMin = tilemaps[0].GetCellCenterWorld(cellBoundsRect.min.ToVec3Int()) - (tilemaps[0].cellSize / 2);
			Vector2 worldBoundsMax = tilemaps[0].GetCellCenterWorld(cellBoundsRect.max.ToVec3Int()) + (tilemaps[0].cellSize / 2);
			worldBoundsRect = Rect.MinMaxRect(worldBoundsMin.x, worldBoundsMin.y, worldBoundsMax.x, worldBoundsMax.y);
		}

		void OnDisable ()
		{
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}
#endif

		void OnDestroy ()
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

		public virtual void SetPieces ()
		{
			pieces = new WorldPiece[maxPieceLocation.x + 1, maxPieceLocation.y + 1];
			// piecesDict.Clear();
			for (int i = 0; i < piecesParent.childCount; i ++)
			{
				WorldPiece piece = piecesParent.GetChild(i).GetComponent<WorldPiece>();
				// piecesDict.Add(piece.location, piece);
				pieces[piece.location.x, piece.location.y] = piece;
			}
		}

		public virtual void Init ()
		{
			if (!enabled)
				return;
			SetPieces ();
			piecesNearPlayer.Clear();
			for (int i = 0; i < activePieces.Count; i ++)
			{
				WorldPiece piece = activePieces[i];
				piece.gameObject.SetActive(false);
			}
			activePieces.Clear();
			for (int x = 0; x <= maxPieceLocation.x; x ++)
			{
				for (int y = 0; y <= maxPieceLocation.y; y ++)
				{
					WorldPiece piece = pieces[x, y];
					if (piece.worldBoundsRect.Contains(Player.instance.trs.position.ToVec2Int()))
					{
						surroundingPieces = GetSurroundingPieces(piece);
						for (int i = 0; i < surroundingPieces.Length; i ++)
						{
							WorldPiece surroundingPiece = surroundingPieces[i];
							loadPieceRangeRect = surroundingPiece.worldBoundsRect.Expand(loadPiecesRange * 2);
							if (GameCamera.Instance.viewRect.IsIntersecting(loadPieceRangeRect))
							{
								piecesNearPlayer.Add(surroundingPiece);
								if (!WorldMap.foundWorldPiecesLocations.Contains(surroundingPiece.location))
									WorldMap.foundWorldPiecesLocations.Add(surroundingPiece.location);
								activePieces.Add(surroundingPiece);
								surroundingPiece.gameObject.SetActive(true);
							}
						}
						piecesNearPlayer.Add(piece);
						if (!WorldMap.foundWorldPiecesLocations.Contains(piece.location))
							WorldMap.foundWorldPiecesLocations.Add(piece.location);
						activePieces.Add(piece);
						piece.gameObject.SetActive(true);
						piecesNearPlayer.AddRange(GetSurroundingPieces(activePieces.ToArray()));
						return;
					}
				}
			}
		}

		public virtual void DoUpdate ()
		{
			for (int i = 0; i < piecesNearPlayer.Count; i ++)
			{
				WorldPiece pieceNearPlayer = piecesNearPlayer[i];
				loadPieceRangeRect = pieceNearPlayer.worldBoundsRect.Expand(loadPiecesRange * 2);
				bool pieceShouldBeActive = GameCamera.Instance.viewRect.IsIntersecting(loadPieceRangeRect);
				if (!pieceShouldBeActive)
				{
					for (int i2 = 0; i2 < Arrow.shotArrows.Count; i2 ++)
					{
						Arrow arrow = Arrow.shotArrows[i2];
						if (loadPieceRangeRect.IsIntersecting(arrow.collider.bounds.ToRect()))
						{
							pieceShouldBeActive = true;
							break;
						}
					}
				}
				if (pieceShouldBeActive)
				{
					if (!pieceNearPlayer.gameObject.activeSelf)
					{
						pieceNearPlayer.gameObject.SetActive(true);
						if (!WorldMap.foundWorldPiecesLocations.Contains(pieceNearPlayer.location))
							WorldMap.foundWorldPiecesLocations.Add(pieceNearPlayer.location);
						activePieces.Add(pieceNearPlayer);
						surroundingPieces = GetSurroundingPieces(pieceNearPlayer);
						for (int i2 = 0; i2 < surroundingPieces.Length; i2 ++)
						{
							WorldPiece surroundingPiece = surroundingPieces[i2];
							if (!piecesNearPlayer.Contains(surroundingPiece))
								piecesNearPlayer.Add(surroundingPiece);
						}
					}
				}
				else if (pieceNearPlayer.gameObject.activeSelf)
				{
					pieceNearPlayer.gameObject.SetActive(false);
					activePieces.Remove(pieceNearPlayer);
				}
				else if (!GetSurroundingPieces(activePieces.ToArray()).Contains(pieceNearPlayer))
				{
					piecesNearPlayer.RemoveAt(i);
					i --;
				}
			}
		}

		public virtual WorldPiece[] GetSurroundingPieces (params WorldPiece[] innerPieces)
		{
			List<WorldPiece> output = new List<WorldPiece>();
			for (int i = 0; i < innerPieces.Length; i ++)
			{
				WorldPiece piece = innerPieces[i];
				bool hasPieceRight = piece.location.x < maxPieceLocation.x;
				bool hasPieceLeft = piece.location.x > 0;
				bool hasPieceUp = piece.location.y < maxPieceLocation.y;
				bool hasPieceDown = piece.location.y > 0;
				if (hasPieceUp)
				{
					output.Add(pieces[piece.location.x, piece.location.y + 1]);
					if (hasPieceRight)
						output.Add(pieces[piece.location.x + 1, piece.location.y + 1]);
					if (hasPieceLeft)
						output.Add(pieces[piece.location.x - 1, piece.location.y + 1]);
				}
				if (hasPieceDown)
				{
					output.Add(pieces[piece.location.x, piece.location.y - 1]);
					if (hasPieceRight)
						output.Add(pieces[piece.location.x + 1, piece.location.y - 1]);
					if (hasPieceLeft)
						output.Add(pieces[piece.location.x - 1, piece.location.y - 1]);
				}
				if (hasPieceRight)
					output.Add(pieces[piece.location.x + 1, piece.location.y]);
				if (hasPieceLeft)
					output.Add(pieces[piece.location.x - 1, piece.location.y]);
				for (int i2 = 0; i2 < innerPieces.Length; i2 ++)
				{
					WorldPiece piece2 = innerPieces[i2];
					output.Remove(piece2);
				}
			}
			return output.ToArray();
		}
	}
}