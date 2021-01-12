using UnityEngine;
using Extensions;

public class Vector2IntRange : Range<Vector2Int>
{
	public static Vector2IntRange NULL = new Vector2IntRange(VectorExtensions.NULL2INT, VectorExtensions.NULL2INT);

	public Vector2IntRange (Vector2Int min, Vector2Int max) : base (min, max)
	{
	}
}