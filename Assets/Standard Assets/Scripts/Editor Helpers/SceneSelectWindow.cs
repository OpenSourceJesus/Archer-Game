#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSelectWindow : EditorWindow
{
	[MenuItem("Window/Scene Select")]
	public static void Init ()
	{
		SceneSelectWindow window = (SceneSelectWindow) EditorWindow.GetWindow(typeof(SceneSelectWindow));
		window.Show();
	}

	public virtual void OnGUI ()
	{
		GUIContent guiContent;
		for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i ++)
		{
			guiContent = new GUIContent();
			guiContent.text = EditorSceneManager.GetSceneByBuildIndex(i).name;
			EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
		}
	}
}
#endif