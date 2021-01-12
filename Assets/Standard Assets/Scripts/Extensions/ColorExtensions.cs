using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace Extensions
{
	public static class ColorExtensions
	{
		public static Color NULL = new Color(MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT);
		public static Color32 WHITE = new Color32(255, 255, 255, 255);
		public static Color32 HALF = new Color32(128, 128, 128, 128);
		public static Color32 CLEAR = new Color32(0, 0, 0, 0);

		public static Color SetAlpha (this Color c, float a)
		{
			return new Color(c.r, c.g, c.b, a);
		}
		
		public static Color AddAlpha (this Color c, float a)
		{
			return SetAlpha (c, c.a + a);
		}
		
		public static Color MultiplyAlpha (this Color c, float a)
		{
			return SetAlpha (c, c.a * a);
		}
		
		public static Color DivideAlpha (this Color c, float a)
		{
			return SetAlpha (c, c.a / a);
		}
		
		public static Color Add (this Color c, Color add)
		{
			return new Color(c.r + add.r, c.g + add.g, c.b + add.b, c.a + add.a);
		}
		
		public static Color Multiply (this Color c, Color multiply)
		{
			return new Color(c.r * multiply.r, c.g * multiply.g, c.b * multiply.b, c.a * multiply.a);
		}
		
		public static Color Add (this Color c, float add)
		{
			return new Color(c.r + add, c.g + add, c.b + add, c.a + add);
		}
		
		public static Color Multiply (this Color c, float multiply)
		{
			return new Color(c.r * multiply, c.g * multiply, c.b * multiply, c.a * multiply);
		}
		
		public static Color Divide (this Color c, float divide)
		{
			return new Color(c.r / divide, c.g / divide, c.b / divide, c.a / divide);
		}

		public static Color RandomColor ()
		{
			return new Color(Random.value, Random.value, Random.value);
		}

		public static Color GetAverage (params Color[] colors)
		{
			Color output = CLEAR;
			foreach (Color color in colors)
				output = output.Add(color);
			return output.Divide(colors.Length);
		}

		public static Color GetLerpAverage (params Color[] colors)
		{
			Color output = colors[0];
			for (int i = 1; i < colors.Length; i ++)
				output = Color.Lerp(output, colors[i], 1f / colors.Length);
			return output;
		}

		public static float GetBrightness (this Color c)
		{
			return (c.r + c.g + c.b) / 3;
		}

		public static float GetSimilarity (this Color color, Color otherColor, bool testAlpha)
		{
			float rDifference = Mathf.Abs(color.r - otherColor.r);
			float gDifference = Mathf.Abs(color.g - otherColor.g);
			float bDifference = Mathf.Abs(color.b - otherColor.b);
			if (testAlpha)
			{
				float aDifference = Mathf.Abs(color.a - otherColor.a);
				return 1f - (rDifference + gDifference + bDifference + aDifference) / 4;
			}
			else
			{
				return 1f - (rDifference + gDifference + bDifference) / 3;
			}
		}

		public static Color GetOpposite (this Color color)
		{
			Color output = color;
			color.r = 1f - color.r;
			color.g = 1f - color.g;
			color.b = 1f - color.b;
			color.a = 1f - color.a;
			return output;
		}
	}
}