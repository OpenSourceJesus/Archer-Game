using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class MapFragment : Collectible
	{
#if UNITY_EDITOR
		public Collider2D[] exploreColiders = new Collider2D[0];
		public int checksPerUnit;
		public bool update;
#endif
		public Vector2Int[] exploreSpaces = new Vector2Int[0];

#if UNITY_EDITOR
		void OnEnable ()
		{
			if (!update|| !EditorApplication.isPlaying)
				return;
			update = false;
			SetExploreSpaces ();
			// EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			// EditorApplication.isPlaying = true;
		}
		
		// void OnPlayModeStateChanged (PlayModeStateChange state)
		// {
		// 	// EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		// 	print(state);
		// 	SetExploreSpaces ();
		// 	// MapFragment[] mapFragments = FindObjectsOfType<MapFragment>();
		// 	// foreach (MapFragment mapFragment in mapFragments)
		// 	// 	mapFragment.SetExploreSpaces ();
		// }

		void SetExploreSpaces ()
		{
			List<Vector2Int> _exploreSpaces = new List<Vector2Int>();
			foreach (Collider2D exploreCollider in exploreColiders)
			{
				Rect exploreColliderRect = exploreCollider.bounds.ToRect();
				for (float x = exploreColliderRect.xMin; x < exploreColliderRect.xMax; x += 1f / checksPerUnit)
				{
					for (float y = exploreColliderRect.yMin; y < exploreColliderRect.yMax; y += 1f / checksPerUnit)
					{
						Vector2 checkPosition = new Vector2(x, y);
						Vector2Int checkCellPosition = (Vector2Int) World.Instance.tilemaps[0].WorldToCell(checkPosition);
						if (exploreCollider.OverlapPoint(checkPosition) && !_exploreSpaces.Contains(checkCellPosition))
							_exploreSpaces.Add(checkCellPosition);
					}
				}
			}
			exploreSpaces = _exploreSpaces.ToArray();
		}
#endif
		public override void OnTriggerEnter2D (Collider2D other)
		{
			base.OnTriggerEnter2D (other);
			foreach (Vector2Int exploreSpace in exploreSpaces)
				WorldMap.exploredCellPositions.Add(exploreSpace);
		}
	}
}