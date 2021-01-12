#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace ArcherGame
{
	[CustomGridBrush(false, false, false, "Line Brush")]
	public class LineBrush : GridBrush
	{
		public static bool shouldSnapAngle;
		public float snapAngle;
		public bool lineStartActive;
		public RectInt lineStart;
		public RectInt lineEnd;
		public Vector2Int brushSize = Vector2Int.one;
		
		public override void Paint (GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			if (lineStartActive)
			{
				Vector2Int startPositionOnLine = new Vector2Int((int) lineStart.center.x, (int) lineStart.center.y);
				Vector2Int endPositionOnLine = new Vector2Int(position.x, position.y);
				if (startPositionOnLine == endPositionOnLine)
					base.Paint(grid, brushTarget, position);    
				else
				{
					foreach (var point in GetPointsOnLine(startPositionOnLine, endPositionOnLine))
					{
						Vector3Int paintPos = new Vector3Int(point.x, point.y, position.z);
						for (int x = paintPos.x - lineStart.width / 2; x < paintPos.x + lineStart.width / 2; x ++)
						{
							for (int y = paintPos.y - lineStart.height / 2; y < paintPos.y + lineStart.height / 2; y ++)
							{
								
							}
						}
						base.Paint(grid, brushTarget, paintPos);
					}
				}
				lineStartActive = false;
			}
			else
			{
				lineStart = new RectInt((Vector2Int) position, brushSize);
				lineStartActive = true;
			}
		}

		public override void Select (GridLayout grid, GameObject brushTarget, BoundsInt position)
		{
			brushSize = new Vector2Int(position.size.x / 2, position.size.y / 2);
		}

		[MenuItem("Tools/Snap Line Brush Angle %#&s")]
		public static void SnapLineBrushAngle ()
		{
			shouldSnapAngle = true;
		}

		public static IEnumerable<Vector2Int> GetPointsOnLine (Vector2Int p1, Vector2Int p2)
		{
			int x0 = p1.x;
			int y0 = p1.y;
			int x1 = p2.x;
			int y1 = p2.y;
			bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				int t;
				t = x0; // swap x0 and y0
				x0 = y0;
				y0 = t;
				t = x1; // swap x1 and y1
				x1 = y1;
				y1 = t;
			}
			if (x0 > x1)
			{
				int t;
				t = x0; // swap x0 and x1
				x0 = x1;
				x1 = t;
				t = y0; // swap y0 and y1
				y0 = y1;
				y1 = t;
			}
			int dx = x1 - x0;
			int dy = Math.Abs(y1 - y0);
			int error = dx / 2;
			int ystep = (y0 < y1) ? 1 : -1;
			int y = y0;
			for (int x = x0; x <= x1; x++)
			{
				yield return new Vector2Int((steep ? y : x), (steep ? x : y));
				error = error - dy;
				if (error < 0)
				{
					y += ystep;
					error += dx;
				}
			}
			yield break;
		}
	}

	// [CustomEditor(typeof(LineBrush))]
	// public class LineBrushEditor : Editor
	// {
	//     private LineBrush lineBrush { get { return target as LineBrush; } }

	//     public virtual void OnSceneGUI ()
	//     {
	//         if (lineBrush.lineStartActive)
	//         {
	//             Tilemap tilemap = GridBrush.ActiveTilemap;
	//             if (tilemap != null)
	//                 tilemap.ClearAllEditorPreviewTiles();
	//             // Draw preview tiles for tilemap
	//             Vector2Int startPos = new Vector2Int(lineBrush.lineStart.x, lineBrush.lineStart.y);
	//             Vector2Int endPos = new Vector2Int(position.x, position.y);
	//             if (startPos == endPos)
	//                 PaintPreview(grid, brushTarget, position.min);
	//             else
	//             {
	//                 foreach (var point in LineBrush.GetPointsOnLine(startPos, endPos))
	//                 {
	//                     Vector3Int paintPos = new Vector3Int(point.x, point.y, position.z);
	//                     PaintPreview(grid, brushTarget, paintPos);
	//                 }
	//             }
	//             if (Event.current.type == EventType.Repaint)
	//             {
	//                 var min = lineBrush.lineStart;
	//                 var max = lineBrush.lineStart + position.size;
	//                 // Draws a box on the picked starting position
	//                 GL.PushMatrix();
	//                 GL.MultMatrix(GUI.matrix);
	//                 GL.Begin(GL.LINES);
	//                 Handles.color = Color.blue;
	//                 Handles.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z));
	//                 Handles.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z));
	//                 Handles.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z));
	//                 Handles.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, min.y, min.z));
	//                 GL.End();
	//                 GL.PopMatrix();
	//             }
	//         }
	//     }
	// }
}
#endif
