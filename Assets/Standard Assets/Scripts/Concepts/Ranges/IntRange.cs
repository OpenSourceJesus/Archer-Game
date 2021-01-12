using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

[Serializable]
public class IntRange : Range<int>
{
	public static IntRange NULL = new IntRange(MathfExtensions.NULL_INT, MathfExtensions.NULL_INT);

	public IntRange (int min, int max) : base (min, max)
	{
	}

	public bool DoesIntersect (IntRange intRange, bool equalIntsIntersect = true)
	{
		if (equalIntsIntersect)
			return (min >= intRange.min && min <= intRange.max) || (intRange.min >= min && intRange.min <= max) || (max <= intRange.max && max >= intRange.min) || (intRange.max <= max && intRange.max >= min);
		else
			return (min > intRange.min && min < intRange.max) || (intRange.min > min && intRange.min < max) || (max < intRange.max && max > intRange.min) || (intRange.max < max && intRange.max > min);
	}

	public bool GetIntersectionRange (IntRange intRange, out IntRange intersectionRange, bool equalIntsIntersect = true)
	{
		intersectionRange = NULL;
		if (DoesIntersect(intRange, equalIntsIntersect))
			intersectionRange = new IntRange(Mathf.Max(min, intRange.min), Mathf.Min(max, intRange.max));
		return intersectionRange != NULL;
	}

	public override int Get (float normalizedValue)
	{
		return Mathf.RoundToInt((max - min) * normalizedValue + min);
	}
}