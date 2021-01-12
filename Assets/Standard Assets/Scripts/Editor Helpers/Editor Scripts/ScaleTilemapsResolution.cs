#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Extensions;

public class ScaleTilemapsResolution : EditorScript
{
	public Transform trs;
	Tilemap[] tilemaps = new Tilemap[0];
	public IntOrReciprocal scale;
	public bool update;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			return;
		}
	}

	public override void DoEditorUpdate ()
	{
		if (!update)
			return;
		update = false;
		tilemaps = GetComponentsInChildren<Tilemap>();
		BoundsInt boundsInt;
		Tilemap newTilemap;
		GameObject newGo;
		TileBase tile;
		Vector2Int fillStart = new Vector2Int();
		Vector2Int fillEnd = new Vector2Int();
		Transform newTrs = Instantiate(trs);
		newTrs.localScale *= scale.GetValue();
		Tilemap[] newTilemaps = newTrs.GetComponentsInChildren<Tilemap>();
		newTrs.position += tilemaps[0].CellToWorld(Vector3Int.one) - newTilemaps[0].CellToWorld(Vector3Int.one);
		if (scale.integer >= 4)
		{
			for (int i2 = 4; i2 <= scale.integer; i2 += 2)
				newTrs.position -= Vector3.one * scale.GetValue();
		}
		Tilemap tilemap;
		Vector3Int cellPosition;
		Vector3 localPosition;
		for (int i = 0; i < tilemaps.Length; i ++)
		{
			tilemap = tilemaps[i];
			newTilemap = newTilemaps[i];
			newTilemap.ClearAllTiles();
			boundsInt = tilemap.cellBounds;
			for (int x = boundsInt.min.x; x < boundsInt.max.x; x ++)
			{
				for (int y = boundsInt.min.y; y < boundsInt.max.y; y ++)
				{
					cellPosition = new Vector3Int(x, y, 0);
					tile = tilemap.GetTile(cellPosition);
					if (tile != null)
					{
						for (int x2 = 0; x2 < scale.integer; x2 ++)
						{
							for (int y2 = 0; y2 < scale.integer; y2 ++)
							{
								localPosition = tilemap.CellToLocalInterpolated(new Vector3(cellPosition.x + Mathf.Lerp(-.5f, .5f, x2 * scale.GetValue()), cellPosition.y + Mathf.Lerp(-.5f, .5f, y2 * scale.GetValue())));
								newTilemap.SetTile(newTilemap.LocalToCell(localPosition * scale.integer), tile);
							}
						}
					}
				}
			}
		}
	}
}
#endif