#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Extensions;
using UnityEditor;

namespace ArcherGame
{
	[CustomGridBrush(false, false, false, "Hollow Brush")]
	public class HollowBrush : GridBrush
	{
		public override void Paint (GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			base.Paint(grid, brushTarget, position);
		}
	}
}
#endif