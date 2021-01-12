using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.UI;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class BuildManager : SingletonMonoBehaviour<BuildManager>
	{
#if UNITY_EDITOR
		public BuildAction[] buildActions;
		static BuildPlayerOptions buildOptions;
		public Text versionNumberText;
#endif
		public int versionIndex;
		public string versionNumberPrefix;
		public GameObject devOptionsGo;
		
		public virtual void Start ()
		{
			devOptionsGo.SetActive(SystemInfo.deviceUniqueIdentifier == "a43c6de0f2f86a8c5798f4a46e938fc62982ed00");
		}

#if UNITY_EDITOR
		public static string[] GetScenePathsInBuild ()
		{
			List<string> scenePathsInBuild = new List<string>();
			string scenePath = null;
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i ++)
			{
				scenePath = EditorBuildSettings.scenes[i].path;
				if (EditorBuildSettings.scenes[i].enabled)
					scenePathsInBuild.Add(scenePath);
			}
			return scenePathsInBuild.ToArray();
		}

		public static string[] GetAllScenePaths ()
		{
			List<string> scenePathsInBuild = new List<string>();
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i ++)
				scenePathsInBuild.Add(EditorBuildSettings.scenes[i].path);
			return scenePathsInBuild.ToArray();
		}
		
		[MenuItem("Build/Make Builds")]
		public static void Build ()
		{
			BuildManager.Instance._Build ();
		}

		public virtual void _Build ()
		{
			BuildManager.Instance.versionIndex ++;
			PreBuildScript[] preBuildScripts = FindObjectsOfType<PreBuildScript>();
			foreach (PreBuildScript preBuildScript in preBuildScripts)
			{
				if (preBuildScript.enabled)
					preBuildScript.Do ();
			}
			foreach (BuildAction buildAction in buildActions)
			{
				if (buildAction.enabled)
					buildAction.Do ();
			}
			PostBuildScript[] postBuildScripts = FindObjectsOfType<PostBuildScript>();
			foreach (PostBuildScript postBuildScript in postBuildScripts)
			{
				if (postBuildScript.enabled)
					postBuildScript.Do ();
			}
		}
		
		[Serializable]
		public class BuildAction
		{
			public string name;
			public bool enabled;
			public BuildTarget target;
			public string locationPath;
			public BuildOptions[] options;
			public bool makeZip;
			public string directoryToZip;
			public string zipLocationPath;
			public bool moveCrashHandler;
			
			public virtual void Do ()
			{
				EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
				WorldMakerWindow.SetWorldActive (false);
				EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
				if (BuildManager.Instance.versionNumberText != null)
					BuildManager.Instance.versionNumberText.text = BuildManager.Instance.versionNumberPrefix + DateTime.Now.Date.ToString("MMdd");
				if (ConfigurationManager.Instance != null)
					ConfigurationManager.Instance.canvas.gameObject.SetActive(false);
				EditorSceneManager.MarkAllScenesDirty();
				EditorSceneManager.SaveOpenScenes();
				buildOptions = new BuildPlayerOptions();
				buildOptions.scenes = GetScenePathsInBuild();
				buildOptions.target = target;
				buildOptions.locationPathName = locationPath;
				foreach (BuildOptions option in options)
					buildOptions.options |= option;
				BuildPipeline.BuildPlayer(buildOptions);
				if (ConfigurationManager.Instance != null)
					ConfigurationManager.Instance.canvas.gameObject.SetActive(true);
				AssetDatabase.Refresh();
				if (moveCrashHandler)
				{
					string extrasPath = locationPath + "/Extras";
					string crashHandlerFileName = "UnityCrashHandler64.exe";
					if (!Directory.Exists(extrasPath))
						Directory.CreateDirectory(extrasPath);
					if (File.Exists(extrasPath + "/" + crashHandlerFileName))
						File.Delete(extrasPath + "/" + crashHandlerFileName);
					else
					{
						crashHandlerFileName = "UnityCrashHandler32.exe";
						if (File.Exists(extrasPath + "/" + crashHandlerFileName))
							File.Delete(extrasPath + "/" + crashHandlerFileName);
					}
					File.Move(locationPath + "/" + crashHandlerFileName, extrasPath + "/" + crashHandlerFileName);
				}
				if (makeZip)
				{
					File.Delete(zipLocationPath);
					DirectoryCompressionOperations.CompressDirectory (directoryToZip, zipLocationPath);
				}
			}
		}
#endif
	}
}