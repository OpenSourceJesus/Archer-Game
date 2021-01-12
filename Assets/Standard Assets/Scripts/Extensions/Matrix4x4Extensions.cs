using UnityEngine;

namespace Extensions
{
	public static class Matrix4x4Extensions
	{
		public static Vector3 GetPosition (this Matrix4x4 matrix)
		{
			return matrix.GetColumn(3);
		}

		public static Quaternion GetRotation (this Matrix4x4 matrix)
		{
			return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
		}

		public static Vector3 GetLocalScale (this Matrix4x4 matrix)
		{
			return new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
		}

		public static Matrix4x4 GetCopy (this Matrix4x4 matrix)
		{
			Matrix4x4 output = new Matrix4x4();
			for (int x = 0; x < 4; x ++)
			{
				for (int y = 0; y < 4; y ++)
					output[x, y] = matrix[x, y];
			}
			return output;
		}
	}
}