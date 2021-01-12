#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Extensions;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class RedirectSelection : MonoBehaviour
	{
		public Object[] newSelection = new Object[0];

		[MenuItem("Tools/Redirect Selection")]
		public static void _RedirectSelection ()
		{
			for (int i = 0; i < Selection.objects.Length; i ++)
			{
				Object obj = Selection.objects[i];
				GameObject go = obj as GameObject;
				if (go != null)
				{
					RedirectSelection redirectSelection = go.GetComponent<RedirectSelection>();
					if (redirectSelection != null)
					{
						Selection.objects = Selection.objects.RemoveAt(i);
						Selection.objects = Selection.objects.AddRange(redirectSelection.newSelection);
					}
				}
			}
		}
	}
}
#endif