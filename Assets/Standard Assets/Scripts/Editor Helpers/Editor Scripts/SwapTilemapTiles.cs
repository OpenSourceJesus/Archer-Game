#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class SwapTilemapTiles : EditorScript
{
	public Tilemap tilemap;
	public Tile tile;
	public Tile tile2;
	public bool update;

	public override void DoEditorUpdate ()
	{
		if (!update)
			return;
		update = false;
		tilemap.SwapTile(tile, tile2);
	}
}

[CustomEditor(typeof(SwapTilemapTiles))]
public class SwapTilemapTilesEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class SwapTilemapTiles : EditorScript
{
}
#endif