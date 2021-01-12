#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;
using UnityEditor;
using System;

//[ExecuteInEditMode]
public class DestroyComponentsOfTypes : EditorScript
{
	public Component[] components;
	public Type[] types;

	public virtual void Do ()
	{
		int indexOfNull;
		do
		{
			indexOfNull = components.IndexOf(null);
			if (indexOfNull != -1)
				components = components.RemoveAt(indexOfNull);
			else
				break;
		} while (true);
		if (components.Length > 0)
		{
			types = new Type[0];
			foreach (Component component in components)
				types = types.Add(component.GetType());
		}
		Component[] _components = GetComponents<Component>();
		Component _component;
		foreach (Type type in types)
		{
			for (int i = 0; i < _components.Length; i ++)
			{
				_component = _components[i];
				if (_component.GetType() == type)
				{
					DestroyImmediate(_component);
					_components = _components.RemoveAt(i);
					i --;
				}
			}
		}
	}
}

[CustomEditor(typeof(DestroyComponentsOfTypes))]
public class DestroyComponentsOfTypesEditor : EditorScriptEditor
{
}
#endif
#if !UNITY_EDITOR
public class DestroyComponentsOfTypes : EditorScript
{
}
#endif