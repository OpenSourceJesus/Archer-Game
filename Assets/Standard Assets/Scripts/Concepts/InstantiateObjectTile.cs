using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ArcherGame
{
	[CreateAssetMenu]
	public class InstantiateObjectTile : Tile
	{
		public override bool StartUp (Vector3Int location, ITilemap tilemap, GameObject go)
		{
			if (go != null)
			{
				Transform trs = go.GetComponent<Transform>();
				trs.position = location + new Vector3(0, 0.5f, 0);
				trs.rotation = tilemap.GetTransformMatrix(location).rotation;
				SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
				if (spriteRenderer != null)
				{
					spriteRenderer.material.mainTextureOffset = tilemap.GetTransformMatrix(location).lossyScale;
					spriteRenderer.color = tilemap.GetComponent<Tilemap>().color;
					spriteRenderer.sortingOrder += (int) tilemap.GetTransformMatrix(location).m23;
				}
			}
			return true;
		}
	}
}