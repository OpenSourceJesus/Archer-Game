using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
	public static class BoundsExtensions
	{
		public static Bounds NULL = new Bounds(VectorExtensions.NULL3, VectorExtensions.NULL3);

		public static bool IsEncapsulating (this Bounds b1, Bounds b2, bool equalBoundsRetunsTrue)
		{
			if (equalBoundsRetunsTrue)
			{
				bool minIsOk = b1.min.x <= b2.min.x && b1.min.y <= b2.min.y && b1.min.z <= b2.min.z;
				bool maxIsOk = b1.min.x >= b2.min.x && b1.min.y >= b2.min.y && b1.min.z >= b2.min.z;
				return minIsOk && maxIsOk;
			}
			else
			{
				bool minIsOk = b1.min.x < b2.min.x && b1.min.y < b2.min.y && b1.min.z < b2.min.z;
				bool maxIsOk = b1.max.x > b2.max.x && b1.max.y > b2.max.y && b1.max.z > b2.max.z;
				return minIsOk && maxIsOk;
			}
		}
		
		public static Bounds Combine (Bounds[] boundsArray)
		{
			Bounds output = boundsArray[0];
			for (int i = 1; i < boundsArray.Length; i ++)
			{
				if (boundsArray[i].min.x < output.min.x)
					output.min = new Vector3(boundsArray[i].min.x, output.min.y, output.min.z);
				if (boundsArray[i].min.y < output.min.y)
					output.min = new Vector3(output.min.x, boundsArray[i].min.y, output.min.z);
				if (boundsArray[i].min.z < output.min.z)
					output.min = new Vector3(output.min.x, output.min.y, boundsArray[i].min.z);
				if (boundsArray[i].max.x > output.max.x)
					output.max = new Vector3(boundsArray[i].max.x, output.max.y, output.max.z);
				if (boundsArray[i].max.y > output.max.y)
					output.max = new Vector3(output.max.x, boundsArray[i].max.y, output.max.z);
				if (boundsArray[i].max.z > output.max.z)
					output.max = new Vector3(output.max.x, output.max.y, boundsArray[i].max.z);
			}
			return output;
		}
		
		public static bool Intersects (Bounds b1, Bounds b2, Vector3 expandB1 = new Vector3(), Vector3 expandB2 = new Vector3())
		{
			b1.Expand(expandB1);
			b2.Expand(expandB2);
			return b1.Intersects(b2);
		}
		
		public static Vector3 RandomPoint (this Bounds bounds)
		{
			return new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
		}
	}
}