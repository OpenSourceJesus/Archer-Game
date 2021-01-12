#if UNITY_EDITOR
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public static class TilePalette
{
	public static GameObject GetTargetTilemap ()
	{
		Assembly assembly = Assembly.Load(new AssemblyName("UnityEditor"));
		Type type = assembly.GetType("UnityEditor.GridPaintingState");
		PropertyInfo scenePaintTargetProperty = type.GetProperty("scenePaintTarget", BindingFlags.Static | BindingFlags.Public);
		return (GameObject) scenePaintTargetProperty.GetValue(null, null);
	}

	public static GridBrushBase GetActiveBrush ()
	{
		Assembly assembly = Assembly.Load(new AssemblyName("UnityEditor"));
		Type type = assembly.GetType("UnityEditor.GridPaintingState");
		PropertyInfo gridBrushProperty = type.GetProperty("gridBrush", BindingFlags.Static | BindingFlags.Public);
		return (GridBrushBase) gridBrushProperty.GetValue(null, null);
	}
	
	public static void SelectTarget (GameObject target)
	{
		EditorWindow window;
		Type windowType;
		GetWindow(out window, out windowType);
		MethodInfo selectTargetMethod = windowType.GetMethod("SelectTarget", BindingFlags.Instance | BindingFlags.NonPublic);
		selectTargetMethod.Invoke(window, new object[] { 0, target });
	}

	public static void SelectPalette (GameObject palette)
	{
		EditorWindow window;
		Type windowType;
		GetWindow(out window, out windowType);
		PropertyInfo scenePaintTargetProperty = windowType.GetProperty("palette", BindingFlags.Instance | BindingFlags.Public);
		scenePaintTargetProperty.SetValue(window, palette, null);
	}

	public static void SelectBrush (int brushIndex)
	{
		EditorWindow window;
		Type windowType;
		GetWindow(out window, out windowType);
		MethodInfo selectTargetMethod = windowType.GetMethod("SelectBrush", BindingFlags.Instance | BindingFlags.NonPublic);
		selectTargetMethod.Invoke(window, new object[] { brushIndex, null });
	}
	
	public static void GetWindow (out EditorWindow window, out Type windowType)
	{
		Assembly assembly = Assembly.Load(new AssemblyName("UnityEditor"));
		// foreach (Type type in assembly.GetTypes())
		// {
		// 	if (type.IsSubclassOf(typeof(EditorWindow)))
		// 		Debug.Log(type.FullName);
		// }
		// windowType = assembly.GetType("UnityEngine.GridPaintPaletteWindow");
		windowType = assembly.GetType("UnityEditor.GridPalette");
		// Debug.Log(windowType);
		window = EditorWindow.GetWindow(windowType);
	}
}
#endif