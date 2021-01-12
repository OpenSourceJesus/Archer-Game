#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class ReplaceTilemapTilesWithPrefabs : EditorScript
{
	public Tilemap tilemap;
	public Transform prefabsParent;
	public Tile tile;
	public GameObject prefab;
	public bool update;

	public override void DoEditorUpdate ()
	{
		if (!update)
			return;
		update = false;
		Tile _tile;
		foreach (Vector3Int cellPosition in tilemap.cellBounds.allPositionsWithin)
		{
			_tile = tilemap.GetTile(cellPosition) as Tile;
			if (_tile != null && _tile.sprite == tile.sprite)
				Instantiate(prefab, tilemap.GetCellCenterWorld(cellPosition), default(Quaternion), prefabsParent);
		}
	}
}

[CustomEditor(typeof(ReplaceTilemapTilesWithPrefabs))]
public class ReplaceTilemapTilesWithPrefabsEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class ReplaceTilemapTilesWithPrefabs : EditorScript
{
}
#endif