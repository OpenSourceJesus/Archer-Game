#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Extensions;
using ArcherGame;

public class SetActiveTilemapToActiveTile : MonoBehaviour
{
	public Transform tilemapsParent;
	
	public virtual void Do ()
	{
		foreach (Tilemap tilemap in tilemapsParent.GetComponentsInChildren<Tilemap>())
		{
			if (tilemap.GetTile(GridBrush.activeTile.transform.GetColumn(3).ToVec3Int()) != null)
			{
				TilePalette.SelectTarget (tilemap.gameObject);
				return;
			}
		}
	}
}
#endif