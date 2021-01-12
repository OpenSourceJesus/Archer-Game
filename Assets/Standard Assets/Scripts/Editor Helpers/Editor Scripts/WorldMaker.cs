#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Extensions;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using System;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class WorldMaker : EditorScript
	{
		// public static TileGroup tileGroup;
		// public Color gizmosColor;
		// public bool loopBrushes;
		// public int brushIndex;
		// public int[] brushes;
		// public bool loopPaintTargets;
		// public int paintTargetIndex;
		// public GameObject[] paintTargets;
		public TileEntry currentTileEntryType;
		public TileEntry[] tileEntryTypes = new TileEntry[0];
		public static Dictionary<Vector2Int, TileEntry> tileEntriesDict = new Dictionary<Vector2Int, TileEntry>();
		public bool isPainting;
		public Vector2 previousWorldMousePosition;
		public Vector2Int boxFillStartCellPosition;
		public float accuracy;
		public Vector2 worldMousePosition;

		public override void DoEditorUpdate ()
		{
			GridBrush.activeTilemap = GridBrush.ActiveTilemap;
			worldMousePosition = GetMousePositionInWorld();
			if (isPainting)
			{
				SetCurrentTileEntry ();
				PaintLine (previousWorldMousePosition, worldMousePosition);
			}
			previousWorldMousePosition = worldMousePosition;
		}

		public virtual void SetCurrentTileEntry ()
		{
			currentTileEntryType = null;
			for (int i = 0; i < tileEntryTypes.Length; i ++)
			{
				if (tileEntryTypes[i].isCurrent)
				{
					if (currentTileEntryType == null)
						currentTileEntryType = tileEntryTypes[i];
					else
						tileEntryTypes[i].isCurrent = false;
				}
			}
		}

		public virtual void StartPaint ()
		{
			isPainting = true;
		}

		public virtual void StopPaint ()
		{
			isPainting = false;
		}

		public virtual void Paint (Vector2 worldPosition)
		{
			TileEntry tileEntry;
			Vector2Int cellPosition = GridBrush.ActiveTilemap.WorldToCell(worldPosition).ToVec2Int();
			if (!tileEntriesDict.TryGetValue(cellPosition, out tileEntry) || tileEntry.spriteRenderer == null)
			{
				if (tileEntry != null && tileEntry.spriteRenderer == null)
					tileEntriesDict.Remove(cellPosition);
				tileEntry = new TileEntry();
				tileEntry.cellPosition = cellPosition;
				tileEntry.worldPosition = GridBrush.ActiveTilemap.GetCellCenterWorld(cellPosition.ToVec3Int());
				tileEntry.spriteRenderer = Instantiate(currentTileEntryType.spriteRenderer, tileEntry.worldPosition, default(Quaternion), GridBrush.ActiveTilemap.GetComponent<Transform>());
				tileEntry.spriteRenderer.color = GridBrush.ActiveTilemap.color;
				tileEntriesDict.Add(cellPosition, tileEntry);
			}
		}

		// TODO: Test method
		public virtual void Paint (Vector2Int cellPosition)
		{
			TileEntry tileEntry;
			if (!tileEntriesDict.TryGetValue(cellPosition, out tileEntry) || tileEntry.spriteRenderer == null)
			{
				if (tileEntry != null && tileEntry.spriteRenderer == null)
					tileEntriesDict.Remove(cellPosition);
				tileEntry = new TileEntry();
				tileEntry.cellPosition = cellPosition;
				tileEntry.worldPosition = GridBrush.ActiveTilemap.GetCellCenterWorld(cellPosition.ToVec3Int());
				tileEntry.spriteRenderer = Instantiate(currentTileEntryType.spriteRenderer, tileEntry.worldPosition, default(Quaternion), GridBrush.ActiveTilemap.GetComponent<Transform>());
				tileEntry.spriteRenderer.color = GridBrush.ActiveTilemap.color;
				tileEntriesDict.Add(cellPosition, tileEntry);
			}
		}

		public virtual void PaintLine (Vector2 startWorldPosition, Vector2 endWorldPosition)
		{
			LineSegment2D line = new LineSegment2D(startWorldPosition, endWorldPosition);
			for (float distance = 0; distance < line.GetLength(); distance += accuracy)
				Paint (line.GetPointWithDirectedDistance(distance));
			Paint (endWorldPosition);
		}

		public virtual void StartBoxFill ()
		{
			boxFillStartCellPosition = GridBrush.ActiveTilemap.WorldToCell(worldMousePosition).ToVec2Int();
		}

		public virtual void StopBoxFill ()
		{
			Vector2Int boxFillEndCellPosition = GridBrush.ActiveTilemap.WorldToCell(worldMousePosition).ToVec2Int();
			for (int x = Mathf.Min(boxFillStartCellPosition.x, boxFillEndCellPosition.x); x <= Mathf.Max(boxFillStartCellPosition.x, boxFillEndCellPosition.x); x ++)
			{
				for (int y = Mathf.Min(boxFillStartCellPosition.y, boxFillEndCellPosition.y); y <= Mathf.Max(boxFillStartCellPosition.y, boxFillEndCellPosition.y); y ++)
					Paint (new Vector2Int(x, y));
			}
		}

		[Serializable]
		public class TileEntry
		{
			public bool isCurrent;
			public SpriteRenderer spriteRenderer;
			[HideInInspector]
			public Vector2Int cellPosition;
			[HideInInspector]
			public Vector2 worldPosition;
		}

		// public virtual TileGroup GetTileGroupAtMousePosition ()
		// {
		// 	if (GridBrush.ActiveTilemap == null)
		// 		return null;
		// 	TileGroup output = new TileGroup();
		// 	List<TileBase> tiles = new List<TileBase>();
		// 	HashSet<Vector3Int> checkedPositions = new HashSet<Vector3Int>();
		// 	HashSet<Vector3Int> uncheckedPositions = new HashSet<Vector3Int>();
		// 	Vector3Int position = GridBrush.ActiveTilemap.WorldToCell((Vector3) GetMousePositionInWorld());
		// 	uncheckedPositions.Add(position);
		// 	TileBase tile = GridBrush.ActiveTilemap.GetTile(position);
		// 	bool tileAtMouseIsNull = tile == null;
		// 	IEnumerator uncheckedPositionEnumerator = uncheckedPositions.GetEnumerator();
		// 	while (uncheckedPositionEnumerator.MoveNext())
		// 	{
		// 		position = (Vector3Int) uncheckedPositionEnumerator.Current;
		// 		if (!uncheckedPositions.Contains(position + Vector3Int.up) && !checkedPositions.Contains(position + Vector3Int.up) && GridBrush.ActiveTilemap.cellBounds.Contains(position + Vector3Int.up))
		// 			uncheckedPositions.Add(position + Vector3Int.up);
		// 		if (!uncheckedPositions.Contains(position + Vector3Int.right) && !checkedPositions.Contains(position + Vector3Int.right) && GridBrush.ActiveTilemap.cellBounds.Contains(position + Vector3Int.right))
		// 			uncheckedPositions.Add(position + Vector3Int.right);
		// 		if (!uncheckedPositions.Contains(position + Vector3Int.down) && !checkedPositions.Contains(position + Vector3Int.down) && GridBrush.ActiveTilemap.cellBounds.Contains(position + Vector3Int.down))
		// 			uncheckedPositions.Add(position + Vector3Int.down);
		// 		if (!uncheckedPositions.Contains(position + Vector3Int.left) && !checkedPositions.Contains(position + Vector3Int.left) && GridBrush.ActiveTilemap.cellBounds.Contains(position + Vector3Int.left))
		// 			uncheckedPositions.Add(position + Vector3Int.left);
		// 		tile = GridBrush.ActiveTilemap.GetTile(position);
		// 		if (tile == null == tileAtMouseIsNull)
		// 			tiles.Add(tile);
		// 		uncheckedPositions.Remove(position);
		// 		checkedPositions.Add(position);
		// 	}
		// 	output.tiles = tiles.ToArray();
		// 	return output;
		// }
		
		// public virtual void OnDrawGizmos ()
		// {
		// 	Gizmos.color = gizmosColor;
		// 	if (initMousePosition != (Vector2) VectorExtensions.NULL)
		// 		Gizmos.DrawLine(initMousePosition, GetMousePositionInWorld());
		// 	else
		// 	{
		// 	}
		// }

		// [MenuItem("World/Next Brush %&RIGHT")]
		// public static void _NextBrush ()
		// {
		// 	GameManager.GetSingleton<WorldMaker>().NextBrush ();
		// }

		// public virtual void NextBrush ()
		// {
		// 	brushIndex ++;
		// 	if (brushIndex >= brushes.Length)
		// 	{
		// 		if (loopBrushes)
		// 			brushIndex = 0;
		// 		else
		// 			brushIndex = brushes.Length - 1;
		// 	}
		// 	TilePalette.SelectBrush (brushes[brushIndex]);
		// }

		// [MenuItem("World/Previous Brush %&LEFT")]
		// public static void _PreviousBrush ()
		// {
		// 	GameManager.GetSingleton<WorldMaker>().PrevoiusBrush ();
		// }

		// public virtual void PrevoiusBrush ()
		// {
		// 	brushIndex --;
		// 	if (brushIndex < 0)
		// 	{
		// 		if (loopBrushes)
		// 			brushIndex = brushes.Length - 1;
		// 		else
		// 			brushIndex = 0;
		// 	}
		// 	TilePalette.SelectBrush (brushes[brushIndex]);
		// }

		// [MenuItem("World/Next Tilemap %&UP")]
		// public static void _NextTilemap ()
		// {
		// 	GameManager.GetSingleton<WorldMaker>().NextTilemap ();
		// }

		// public virtual void NextTilemap ()
		// {
		// 	paintTargetIndex ++;
		// 	if (paintTargetIndex >= paintTargets.Length)
		// 	{
		// 		if (loopPaintTargets)
		// 			paintTargetIndex = 0;
		// 		else
		// 			paintTargetIndex = paintTargets.Length - 1;
		// 	}
		// 	TilePalette.SelectTarget (paintTargets[paintTargetIndex]);
		// }

		// [MenuItem("World/Previous Tilemap %&DOWN")]
		// public static void _PreviousTilemap ()
		// {
		// 	GameManager.GetSingleton<WorldMaker>().PreviousTilemap ();
		// }

		// public virtual void PreviousTilemap ()
		// {
		// 	paintTargetIndex --;
		// 	if (paintTargetIndex < 0)
		// 	{
		// 		if (loopPaintTargets)
		// 			paintTargetIndex = paintTargets.Length - 1;
		// 		else
		// 			paintTargetIndex = 0;
		// 	}
		// 	TilePalette.SelectTarget (paintTargets[paintTargetIndex]);
		// }

		// public class TileGroup
		// {
		// 	public TileBase[] tiles;
		// 	public BoundsInt bounds;
		// }
	}

	[CustomEditor(typeof(WorldMaker))]
	public class WorldMakerEditor : EditorScriptEditor
	{
	}
}
#endif
#if !UNITY_EDITOR
namespace ArcherGame
{
	public class WorldMaker : EditorScript
	{
	}
}
#endif