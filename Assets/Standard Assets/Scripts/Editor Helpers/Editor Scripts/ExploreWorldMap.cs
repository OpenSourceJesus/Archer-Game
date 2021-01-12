#if UNITY_EDITOR
using ArcherGame;
using UnityEngine;
using UnityEditor;
using Extensions;

public class ExploreWorldMap : EditorScript
{
	public float distanceToExploreFromObelisks;

	public virtual void Do ()
	{
		WorldMap.exploredCellPositions.Clear();
		foreach (Vector2Int cellPosition in World.Instance.cellBoundsRect.allPositionsWithin)
		{
			foreach (Obelisk obelisk in Obelisk.instances)
			{
				if ((cellPosition.ToVec2() - (Vector2) obelisk.trs.position).sqrMagnitude <= distanceToExploreFromObelisks * distanceToExploreFromObelisks)
				{
					WorldMap.exploredCellPositions.Add(cellPosition);
					break;
				}	
			}
		}
		// WorldMap.Instance.minExploredCellPosition = World.Instance.cellBoundsRect.min;
		// WorldMap.Instance.maxExploredCellPosition = World.Instance.cellBoundsRect.max;
	}
}

[CustomEditor(typeof(ExploreWorldMap))]
public class ExploreWorldMapEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
using UnityEngine;

public class ExploreWorldMap : MonoBehaviour
{
}
#endif