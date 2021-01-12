using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Extensions
{
	public static class CollectionExtensions 
	{
		public static T[] Add<T> (this T[] array, T element)
		{
			List<T> output = new List<T>(array);
			output.Add(element);
			return output.ToArray();
		}

		public static T[] Remove<T> (this T[] array, T element)
		{
			List<T> output = new List<T>(array);
			output.Remove(element);
			return output.ToArray();
		}

		public static T[] RemoveAt<T> (this T[] array, int index)
		{
			List<T> output = new List<T>(array);
			output.RemoveAt(index);
			return output.ToArray();
		}

		public static T[] AddRange<T> (this T[] array, IEnumerable<T> array2)
		{
			List<T> output = new List<T>(array);
			output.AddRange(array2);
			return output.ToArray();
		}

		public static T[] AddRange<T> (this T[] array, ICollection<T> array2)
		{
			List<T> output = new List<T>(array);
			output.AddRange(array2);
			return output.ToArray();
		}

		public static T[] AddArray<T> (this T[] array, Array array2)
		{
			List<T> output = new List<T>(array);
			for (int i = 0; i < array2.Length; i ++)
				output.Add((T) array2.GetValue(i));
			return output.ToArray();
		}

		public static bool Contains<T> (this T[] array, T element)
		{
            for (int i = 0; i < array.Length; i ++)
			{
                T obj = array[i];
                if (obj.Equals(element))
					return true;
			}
			return false;
		}

		public static int IndexOf<T> (this T[] array, T element)
		{
			for (int i = 0; i < array.Length; i ++)
			{
				if (array[i].Equals(element))
					return i;
			}
			return -1;
		}
		
		public static T[] Reverse<T> (this T[] array)
		{
			List<T> output = new List<T>(array);
			output.Reverse();
			return output.ToArray();
		}

		public static string ToString<T> (this T[] array, string elementSeperator = ", ")
		{
			string output = "";
			for (int i = 0; i < array.Length; i ++)
			{
				T element = array[i];
				output += element.ToString() + elementSeperator;
			}
			return output;
		}

		public static T[] RemoveEach<T> (this T[] array, IEnumerable<T> array2)
		{
			List<T> output = new List<T>(array);
			foreach (T element in array2)
				output.Remove(element);
			return output.ToArray();
		}

		public static int Count (this IEnumerable enumerable)
		{
			int output = 0;
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
				output ++;
			return output;
		}

		public static T Get<T> (this IEnumerable<T> enumerable, int index)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				index --;
				if (index < 0)
					return (T) enumerator.Current;
			}
			return default(T);
		}

		public static float GetMin (this float[] array)
		{
			float min = array[0];
			for (int i = 1; i < array.Length; i ++)
			{
				float value = array[i];
				if (value < min)
					min = value;
			}
			return min;
		}

		public static float GetMax (this float[] array)
		{
			float max = array[0];
			for (int i = 1; i < array.Length; i ++)
			{
				float value = array[i];
				if (value > max)
					max = value;
			}
			return max;
		}

		public static bool Contains<T> (this UnityEngine.InputSystem.Utilities.ReadOnlyArray<T> array, T element)
		{
            for (int i = 0; i < array.Count; i ++)
			{
                T obj = array[i];
                if (obj.Equals(element))
					return true;
			}
			return false;
		}

		public static T[,] Rotate<T> (this T[,] array, int clockwiseRotations)
		{
			if (clockwiseRotations % 4 == 3)
			{
				T[,] output = new T[array.GetLength(1), array.GetLength(0)];
				for (int x = 0; x < array.GetLength(0); x ++)
				{
					for (int y = 0; y < array.GetLength(1); y ++)
						output[y, x] = array[x, y];
				}
				return output;
			}
			else if (clockwiseRotations % 4 == 2)
			{
				T[,] output = new T[array.GetLength(0), array.GetLength(1)];
				for (int x = 0; x < array.GetLength(0); x ++)
				{
					for (int y = 0; y < array.GetLength(1); y ++)
						output[array.GetLength(0) - x - 1, array.GetLength(1) - y - 1] = array[x, y];
				}
				return output;
			}
			else if (clockwiseRotations % 4 == 1)
			{
				T[,] output = new T[array.GetLength(1), array.GetLength(0)];
				for (int x = 0; x < array.GetLength(0); x ++)
				{
					for (int y = 0; y < array.GetLength(1); y ++)
						output[array.GetLength(1) - y - 1, array.GetLength(0) - x - 1] = array[x, y];
				}
				return output;
			}
			else
				return array;
		}

		public static T[,] InsertColumn<T> (this T[,] array, int index, T[] insert)
		{
			T[,] output = new T[array.GetLength(0) + 1, array.GetLength(1)];
			for (int x = 0; x < output.GetLength(0); x ++)
			{
				if (x == index)
				{
					for (int y = 0; y < insert.Length; y ++)
						output[x, y] = insert[y];
				}
				else if (x > index)
				{
					for (int y = 0; y < array.GetLength(1); y ++)
						output[x, y] = array[x - 1, y];
				}
				else
				{
					for (int y = 0; y < array.GetLength(1); y ++)
						output[x, y] = array[x, y];
				}
			}
			return output;
		}
	}
}