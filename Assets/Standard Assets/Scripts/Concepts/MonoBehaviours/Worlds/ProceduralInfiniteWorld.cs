using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class ProceduralInfiniteWorld : World
	{
		public Vector2IntRange extraRoomSize;
		public Vector2IntRange hallwaySize;
		public Tilemap wallTilemap;

		public override void DoUpdate ()
        {
            base.DoUpdate ();
            
        }

		void MakeRoom (Vector2Int size, Vector2Int minCellPosition)
		{
			int xPosition;
			int yPosition = minCellPosition.y - 1;
			List<Vector3Int> borderPositions = new List<Vector3Int>();
			for (xPosition = minCellPosition.x = 1; xPosition <= minCellPosition.x + size.x + 1; xPosition ++)
				borderPositions.Add(new Vector3Int(xPosition, yPosition, 0));
			for (yPosition = yPosition; yPosition <= minCellPosition.y + size.y + 1; yPosition ++)
				borderPositions.Add(new Vector3Int(xPosition, yPosition, 0));
			for (xPosition = xPosition; xPosition <= minCellPosition.x + size.x + 1; xPosition ++)
				borderPositions.Add(new Vector3Int(xPosition, yPosition, 0));
			for (yPosition = yPosition; yPosition <= minCellPosition.y + size.y + 1; yPosition ++)
				borderPositions.Add(new Vector3Int(xPosition, yPosition, 0));
		}

		public class Room
		{
			public Vector2Int size;
			public List<Hallway> hallways = new List<Hallway>();
		}

		public class Hallway
		{
			public Vector2Int size;
		}
	}
}