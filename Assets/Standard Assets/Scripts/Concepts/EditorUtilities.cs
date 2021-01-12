#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Extensions;
using ArcherGame;

public static class EditorUtilities
{
	[MenuItem("Tools/Make Selected GameObject Names Unique")]
	public static void MakeSelectedGameObjectNamesUnique ()
	{
		List<string> goNames = new List<string>();
		foreach (GameObject go in Selection.gameObjects)
		{
			while (goNames.Contains(go.name))
			{
				int indexOfOpeningParenthases = go.name.IndexOf("(");
				if (indexOfOpeningParenthases != -1)
				{
					int indexOfClosingParenthases = go.name.IndexOf(")");
					int number;
					if (int.TryParse(go.name.SubstringStartEnd(indexOfOpeningParenthases + 1, indexOfClosingParenthases - 1), out number))
					{
						number ++;
						go.name = go.name.RemoveStartEnd(indexOfOpeningParenthases + 1, indexOfClosingParenthases - 1);
						go.name = go.name.Insert(indexOfOpeningParenthases + 1, "" + number);
					}
					else
						go.name += " (1)";
				}
				else
					go.name += " (1)";
			}
			goNames.Add(go.name);
		}
	}

	[MenuItem("Tools/Destroy Children Of Selected")]
	public static void DestroyChildrenOfSelected ()
	{
		int childCount;
		foreach (Transform trs in Selection.transforms)
		{
			childCount = trs.childCount;
			for (int i = 0; i < childCount; i ++)
				GameManager._DestroyImmediate(trs.GetChild(0).gameObject);
		}
	}
}
#endif