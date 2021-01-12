using UnityEngine;

namespace Extensions
{
	public static class MathfExtensions
	{
		public const int NULL_INT = 1234567890;
		public const float NULL_FLOAT = NULL_INT;
		public const float INCHES_TO_CENTIMETERS = 2.54f;
		
		public static float SnapToInterval (float f, float interval)
		{
			if (interval == 0)
				return f;
			else
				return Mathf.Round(f / interval) * interval;
		}

		public static float CeilInterval (float f, float interval)
		{
			return f + (f % interval);
		}
		
		public static int Sign (float f)
		{
			if (f == 0)
				return 0;
			else
				return (int) Mathf.Sign(f);
		}
		
		public static bool AreOppositeSigns (float f1, float f2)
		{
			return Mathf.Abs(Sign(f1) - Sign(f2)) == 2;
		}
		
		public enum RoundingMethod
		{
			HalfOrMoreRoundsUp,
			HalfOrLessRoundsDown,
			RoundUpIfNotWhole,
			RoundDownIfNotWhole
		}

		public static float GetClosestNumber (float f, params float[] numbers)
		{
			float closestNumber = numbers[0];
			float number;
			for (int i = 1; i < numbers.Length; i ++)
			{
				number = numbers[i];
				if (Mathf.Abs(f - number) < Mathf.Abs(f - closestNumber))
					closestNumber = number;
			}
			return closestNumber;
		}

		public static int GetIndexOfClosestNumber (float f, params float[] numbers)
		{
			int indexOfClosestNumber = 0;
			float closestNumber = numbers[0];
			float number;
			for (int i = 1; i < numbers.Length; i ++)
			{
				number = numbers[i];
				if (Mathf.Abs(f - number) < Mathf.Abs(f - closestNumber))
				{
					closestNumber = number;
					indexOfClosestNumber = i;
				}
			}
			return indexOfClosestNumber;
		}

		public static float RegularizeAngle (float angle)
		{
			while (angle >= 360 || angle < 0)
				angle += Mathf.Sign(360 - angle) * 360;
			return angle;
		}

		public static float ClampAngle (float ang, float min, float max)
		{
			ang = WrapAngle(ang);
			min = WrapAngle(min);
			max = WrapAngle(max);
			float minDist = Mathf.Min(Mathf.DeltaAngle(ang, min), Mathf.DeltaAngle(ang, max));
			if (WrapAngle(ang + Mathf.DeltaAngle(ang, minDist)) == min)
				return min;
			else if (WrapAngle(ang + Mathf.DeltaAngle(ang, minDist)) == max)
				return max;
			else
				return ang;
		}

		public static float WrapAngle (float ang)
		{
			if (ang < 0)
				ang += 360;
			else if (ang > 360)
				ang = 360 - ang;
			return ang;
		}
	}
}