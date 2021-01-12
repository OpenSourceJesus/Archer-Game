/*
 * Copyright (c) 2015-2019 Beebyte Limited. All rights reserved.
 */
#if !BEEBYTE_OBFUSCATOR_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using Beebyte.Obfuscator.Assembly;
using UnityEditor;
using UnityEngine;

namespace Beebyte.Obfuscator
{
	/**
	 * Handles obfuscation calls for a Unity project and controls restoration of backed up files.
	 */
	public class Project
	{
		private Options _options;

		private bool _monoBehaviourAssetsNeedReverting;
		private bool _hasError;
		private bool _hasObfuscated;
		private bool _noCSharpScripts;
		
		private bool ShouldObfuscate()
		{
			return _options.enabled && (_options.obfuscateReleaseOnly == false || Debug.isDebugBuild == false);
		}

		public bool IsSuccess()
		{
			return _options && (!ShouldObfuscate() || _hasObfuscated);
		}

		public bool HasCSharpScripts()
		{
            return !_noCSharpScripts;
		}

		public bool HasMonoBehaviourAssetsThatNeedReverting()
		{
			return _monoBehaviourAssetsNeedReverting;
		}

		public void ObfuscateIfNeeded()
		{
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			if (!EditorApplication.isPlayingOrWillChangePlaymode && !_hasObfuscated && _hasError == false)
#else
			if (!EditorApplication.isPlayingOrWillChangePlaymode && !_hasObfuscated && _hasError == false && BuildPipeline.isBuildingPlayer)
#endif
			{
#if !UNITY_5_6_OR_NEWER
				EditorApplication.update += PipelineHook.ClearProjectViaUpdate;
#endif
				try
				{
					EditorApplication.LockReloadAssemblies();
					ObfuscateWhileLocked();
				}
				catch (Exception e)
				{
					Debug.LogError("Obfuscation Failed: " + e);
					_hasError = true;
					throw new OperationCanceledException("Obfuscation failed", e);
				}
				finally
				{
					EditorApplication.UnlockReloadAssemblies();
				}
			}
		}

		private void ObfuscateWhileLocked()
		{
			if (_options == null) _options = OptionsManager.LoadOptions();

			if (ShouldObfuscate() == false) return;

			AssemblySelector selector = new AssemblySelector(_options);

			ICollection<string> compiledDlls = selector.GetCompiledAssemblyPaths();

			if (compiledDlls.Count > 0)
			{
				EditorApplication.update += RestoreUtils.RestoreOriginalDlls;
			}
			
            IDictionary<string, string> backupMap = FileBackup.GetBackupMap(compiledDlls);
			FileBackup.Backup(backupMap);

			ICollection<string> dlls = selector.GetAssemblyPaths();

			if (dlls.Count == 0 && compiledDlls.Count == 0)
			{
				_noCSharpScripts = true;
				return;
			}

			HashSet<string> extraAssemblyReferenceDirectories = new HashSet<string>(_options.extraAssemblyDirectories);
				
#if UNITY_2017_3_OR_NEWER
			extraAssemblyReferenceDirectories.UnionWith(AssemblyReferenceLocator.GetAssemblyReferenceDirectories());
#endif
				
			Obfuscator.SetExtraAssemblyDirectories(extraAssemblyReferenceDirectories.ToArray());
				
#if UNITY_2018_2_OR_NEWER
			if (_options.obfuscateMonoBehaviourClassNames)
			{
				Debug.LogError(
					"The mechanism to obfuscate MonoBehaviour class names no longer works since Unity " +
					"2018.2. You must either roll back to 2018.1, or disable this option.\n" +
					"\nThis build will be obfuscated as instructed, but you are likely to see " +
					"NullReferenceException runtime errors.\n");
			}
#endif

			Obfuscator.Obfuscate(dlls, compiledDlls, _options, EditorUserBuildSettings.activeBuildTarget);

			if (_options.obfuscateMonoBehaviourClassNames)
			{
				/*
                 * RestoreAssets must be registered via the update delegate because [PostProcessBuild] is not guaranteed to be called
                 */
				EditorApplication.update += RestoreUtils.RestoreMonobehaviourSourceFiles;
				_monoBehaviourAssetsNeedReverting = true;
			}
			_hasObfuscated = true;
		}
	}
}
#endif
