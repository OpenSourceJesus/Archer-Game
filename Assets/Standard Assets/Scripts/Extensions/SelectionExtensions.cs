#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Extensions
{
	public class SelectionExtensions
	{
		public static T[] GetSelected<T> () where T : Object
		{
			T[] output = new T[0];
			T obj;
			foreach (Transform trs in Selection.transforms)
			{
				obj = trs.GetComponent<T>();
				if (obj != null)
					output = output.Add(obj);
			}
			return output;
		}
	}
}
#endif