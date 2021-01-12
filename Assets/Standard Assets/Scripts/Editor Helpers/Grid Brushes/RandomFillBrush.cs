#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Tilemaps;
using Extensions;
using Random = UnityEngine.Random;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class RandomFillBrush : GridBrush
	{
		public float blocksSize;
		public float chanceOfBlock;
		Region2Int region;
		public Vector2Int maxRegionSize;

		public virtual void FloodFill (GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (brushTarget != null)
				ActiveTilemap = brushTarget.GetComponent<Tilemap>();
			else if (brushTarget == null && ActiveTilemap != null)
				brushTarget = ActiveTilemap.gameObject;
			region = GetRegionAtMousePosition();
			for (float x = -region.minPositionWithin.x / 2; x < region.minPositionWithin.x / 2; x ++)
			{
				for (float y = -region.maxPositionWithin.y / 2; y < region.maxPositionWithin.y / 2; y ++)
				{
					if (Random.value <= chanceOfBlock)
						base.Paint (gridLayout, brushTarget, position);
				}
			}
		}

		public virtual Region2Int GetRegionAtMousePosition ()
		{
			List<Vector2Int> positionsWithin = new List<Vector2Int>();
			foreach (Vector2Int positionWithin in region.positionsWithin)
			{
				if (GetPath(positionWithin, region.GetCentermostPositionWithin()) != null)
					positionsWithin.Add(positionWithin);
			}
			return new Region2Int(positionsWithin.ToArray());
		}

		public LayerMask whatBlocksMe;

 		public virtual Path GetPath (Vector2Int start, Vector2Int end)
 		{
 			TreeNode<Vector2Int> pathTree = new TreeNode<Vector2Int>(start);
 			Vector2Int[] neighboringPoints = new Vector2Int[4];
 			List<Vector2Int> unvisitedPoints = new List<Vector2Int>();
 			unvisitedPoints.Add(start);
 			do
 			{
 				if (unvisitedPoints[0] == end)
 					break;
 				neighboringPoints[0] = unvisitedPoints[0] + Vector2Int.up;
 				neighboringPoints[1] = unvisitedPoints[0] + Vector2Int.right;
 				neighboringPoints[2] = unvisitedPoints[0] + Vector2Int.down;
 				neighboringPoints[3] = unvisitedPoints[0] + Vector2Int.left;
 				foreach (Vector2Int neighboringPoint in neighboringPoints)
 					ExplorePoint (neighboringPoint, ref pathTree, ref unvisitedPoints);
 				unvisitedPoints.RemoveAt(0);
 			} while (unvisitedPoints.Count > 0);
 			if (unvisitedPoints.Count == 0)
 				return null;
 			List<Vector2Int> points = new List<Vector2Int>();
 			points.Add(end);
 			Path path = new Path();
 			TreeNode<Vector2Int> node = pathTree.GetRoot().GetChild(end);
 			while (node.Parent != null)
 			{
 				node = node.Parent;
 				points.Insert(0, node.Value);
 			}
 			path.points = points.ToArray();
 			return path;
 		}

		public virtual void ExplorePoint (Vector2Int point, ref TreeNode<Vector2Int> pathTree, ref List<Vector2Int> unvisitedPoints)
 		{
 			if (PointIsOpen(point) && !pathTree.GetRoot().Contains(point))
 			{
 				pathTree.GetRoot().GetChild(unvisitedPoints[0]).AddChild(point);
 				unvisitedPoints.Add(point);
 			}
 		}

		public virtual bool PointIsOpen (Vector2Int point)
		{
			return ActiveTilemap.GetTile(point.ToVec3Int());
			// if (Application.isPlaying)
			// 	return Physics2D.OverlapPoint(World.Instance.tilemaps[0].GetCellCenterWorld(point.ToVec3Int()), whatBlocksMe) == null;
			// else
			// {
			// 	foreach (ObjectInWorld worldObject in World.Instance.worldObjects)
			// 	{
			// 		if (worldObject.trs.GetUnrotatedRect().Contains(point) && whatBlocksMe.MaskContainsLayer(worldObject.go.layer))
			// 			return false;
			// 	}
			// 	return !World.Instance.tilemaps[0].HasTile(point.ToVec3Int());
			// }
		}

		 public class Path
 		{
 			public Vector2Int[] points;
 		}

		public class Region2Int
		{
			public Vector2Int[] positionsWithin;
			public Vector2Int minPositionWithin;
			public Vector2Int maxPositionWithin;
			
			public virtual Vector2Int GetCentermostPositionWithin ()
			{
				Vector2Int minPositionWithin = positionsWithin[0];
				Vector2Int maxPositionWithin = positionsWithin[0];
				Vector2Int positionWithin;
				for (int i = 1; i < positionsWithin.Length; i ++)
				{
					positionWithin = positionsWithin[i];
					if (positionWithin.x < minPositionWithin.x || positionWithin.y < minPositionWithin.y)
						minPositionWithin = positionWithin;
					if (positionWithin.x > maxPositionWithin.x || positionWithin.y > maxPositionWithin.y)
						maxPositionWithin = positionWithin;
				}
				return (maxPositionWithin - minPositionWithin) / 2;
			}

			public Region2Int (Vector2Int[] positionWithin)
			{
				this.positionsWithin = positionsWithin;
			}
		}
	}
}
#endif