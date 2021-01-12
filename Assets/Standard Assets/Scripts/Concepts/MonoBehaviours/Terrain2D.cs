using Ferr;
using UnityEngine;
using Extensions;

// [ExecuteInEditMode]
[RequireComponent(typeof(Ferr2DT_PathTerrain))]
public class Terrain2D : MonoBehaviour
{
	public Ferr2DT_PathTerrain terrain;
	public float snapInterval;

	public virtual void Awake ()
	{
// #if UNITY_EDITOR
		// if (Application.isPlaying)
		// {
			// enabled = false;
			// return;
		// }
// #endif
		terrain.Build();
	}

	void OnEnable ()
	{
	}

#if UNITY_EDITOR
	public virtual void OnValidate ()
	{
		for (int i = 0; i < terrain.PathData.Count; i ++)
			terrain.PathData[i] = terrain.PathData[i].Snap(Vector2.one * snapInterval);
	}
#endif
}