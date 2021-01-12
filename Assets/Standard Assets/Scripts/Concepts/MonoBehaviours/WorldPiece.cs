using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class WorldPiece : MonoBehaviour
	{
		public Transform trs;
		public Vector2Int location;
		public Transform tilemapsParent;
		public Transform worldObjectsParent;
		public Tilemap[] tilemaps;
		public RectInt cellBoundsRect;
		public Rect worldBoundsRect;

		public virtual void OnDrawGizmosSelected ()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(cellBoundsRect.center, cellBoundsRect.size.ToVec3());
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(worldBoundsRect.center, worldBoundsRect.size);
		}
	}
}