using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Reflection;
using UnityEngine.SceneManagement;
using Extensions;
using System.IO;

namespace ArcherGame.Analytics
{
	public class AnalyticsManager : SingletonMonoBehaviour<AnalyticsManager>
	{
		public Transform trs;
		public string formUrl;
		UnityWebRequest webRequest;
		WWWForm form;
		public AnalyticsEvent _AnalyticsEvent;
		public PlayerDiedEvent _PlayerDiedEvent;
		public Queue<AnalyticsEvent> eventQueue = new Queue<AnalyticsEvent>();
		[SaveAndLoadValue(false)]
		public bool collectAnalytics;
		[SaveAndLoadValue(false)]
		public bool logAnalyticsLocally;
		[SaveAndLoadValue(false)]
		public int sessionNumber;
		[SaveAndLoadValue(false)]
		public int previousTotalGameplayDuration;
		public static Dictionary<string, AnalyticsManager> analyticsManagers = new Dictionary<string, AnalyticsManager>();
		public Timer sessionTimer;
		public Timer timerSinceLastLog;
		public string localLogFolderPath;
		string currentLogFilePath;
		public const int MAX_STRING_LENGTH = 30;
		public const char FILLER_CHARACTER = ' ';
		public const string VALUE_SEPERATOR = "-----";
		public DataColumn[] dataColumns;
		public Dictionary<string, DataColumn> dataColumnDict = new Dictionary<string, DataColumn>();
		public Dictionary<string, string> columnData = new Dictionary<string, string>();
		
		public override void Awake ()
		{
			if (analyticsManagers.ContainsKey(name))
			{
				Destroy(gameObject);
				return;
			}
			foreach (DataColumn dataColumn in dataColumns)
				dataColumnDict.Add(dataColumn.name, dataColumn);
			localLogFolderPath = Application.persistentDataPath + Path.DirectorySeparatorChar + localLogFolderPath;
			analyticsManagers.Add(name, this);
			trs.SetParent(null);
			base.Awake ();
		}

		public virtual void OnApplicationQuit ()
		{
			previousTotalGameplayDuration += Mathf.RoundToInt(sessionTimer.TimeElapsed);
			sessionNumber ++;
		}
		
		public virtual void LogEvent (AnalyticsEvent _event)
		{
			if (collectAnalytics)
			{
				eventQueue.Enqueue(_event);
				StopCoroutine(LogAllEventsRoutine ());
				StartCoroutine(LogAllEventsRoutine ());
			}
		}
		
		public virtual IEnumerator LogAllEventsRoutine ()
		{
			if (!collectAnalytics)
				yield break;
			while (eventQueue.Count > 0)
				yield return StartCoroutine(LogEventRoutine (eventQueue.Dequeue()));
			yield break;
		}
		
		public virtual IEnumerator LogEventRoutine (AnalyticsEvent _event)
		{
			if (!collectAnalytics)
				yield break;
			_event.LogData (this);
			if (logAnalyticsLocally)
				LogEventLocally (_event);
			yield return StartCoroutine(LogEventOnline(_event));
			yield break;
		}

		public virtual IEnumerator LogEventOnline (AnalyticsEvent _event)
		{
			if (!collectAnalytics)
				yield break;
			using (webRequest = UnityWebRequest.Post(formUrl, form))
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.isNetworkError || webRequest.isHttpError)
					Debug.Log(webRequest.error);
				else
					Debug.Log("Form upload complete!");
			}
			webRequest.Dispose();
			yield break;
		}

		public virtual void LogEventLocally (AnalyticsEvent _event)
		{
			if (!collectAnalytics || !logAnalyticsLocally)
				return;
			if (!Directory.Exists(localLogFolderPath))
				Directory.CreateDirectory(localLogFolderPath);
			currentLogFilePath = localLogFolderPath + Path.DirectorySeparatorChar + "Analytics " + sessionNumber + ".txt";
			string[] currentLogFileLines = new string[1];
			string dataColumn;
			StreamWriter writer = null;
			if (File.Exists(currentLogFilePath))
			{
				currentLogFileLines = File.ReadAllLines(currentLogFilePath);
				File.Delete(currentLogFilePath);
				writer = File.CreateText(currentLogFilePath);
				string currentLogFileLine = "";
				string dataValue;
				for (int i = 0; i < dataColumns.Length; i ++)
				{
					dataColumn = dataColumns[i].name;
					columnData.TryGetValue(dataColumn, out dataValue);
					if (dataValue == null)
						dataValue = "";
					else
						currentLogFileLine += dataValue;
					for (int i2 = dataValue.Length; i2 < MAX_STRING_LENGTH; i2 ++)
						currentLogFileLine += FILLER_CHARACTER;
					currentLogFileLine += VALUE_SEPERATOR;
				}
				currentLogFileLines = currentLogFileLines.Add(currentLogFileLine);
				foreach (string line in currentLogFileLines)
					writer.Write("\n" + line);
			}
			else
			{
				writer = File.CreateText(currentLogFilePath);
				for (int i = 0; i < dataColumns.Length; i ++)
				{
					dataColumn = dataColumns[i].name;
					currentLogFileLines[0] += dataColumn;
					for (int i2 = dataColumn.Length; i2 < MAX_STRING_LENGTH; i2 ++)
						currentLogFileLines[0] += FILLER_CHARACTER;
					currentLogFileLines[0] += VALUE_SEPERATOR;
				}
				writer.Write(currentLogFileLines[0]);
			}
			writer.Close();
			writer.Dispose();
		}
		
		#region Analytics Events
		[Serializable]
		public class AnalyticsEvent
		{
			public GameVersionDataEntry gameVersion = new GameVersionDataEntry();
			public PlayerDataEntry player = new PlayerDataEntry();
			public SceneDataEntry scene = new SceneDataEntry();
			public TimeSinceLastLogDataEntry timeSinceLastLog = new TimeSinceLastLogDataEntry();
			public AnalyticsDataEntry_string eventName = new AnalyticsDataEntry_string();
			public SessionDurationDataEntry sessionDuration = new SessionDurationDataEntry();
			public TotalGameplayDurationDataEntry totalGameplayDuration = new TotalGameplayDurationDataEntry();
			public SessionNumberDataEntry sessionNumber = new SessionNumberDataEntry();
			
			public AnalyticsEvent ()
			{
				Init ();
			}
			
			public virtual void Init ()
			{
				if (AnalyticsManager.Instance == null)
					return;
				AnalyticsEvent _event = (AnalyticsEvent) typeof(AnalyticsManager).GetField("_" + GetName()).GetValue(AnalyticsManager.Instance);
				gameVersion.dataColumnName = _event.gameVersion.dataColumnName;
				player.dataColumnName = _event.player.dataColumnName;
				scene.dataColumnName = _event.scene.dataColumnName;
				timeSinceLastLog.dataColumnName = _event.timeSinceLastLog.dataColumnName;
				eventName.dataColumnName = _event.eventName.dataColumnName;
				sessionDuration.dataColumnName = _event.sessionDuration.dataColumnName;
				totalGameplayDuration.dataColumnName = _event.totalGameplayDuration.dataColumnName;
				sessionNumber.dataColumnName = _event.sessionNumber.dataColumnName;
			}
			
			public virtual string GetName ()
			{
				string output = GetType().ToString();
				output = output.Substring(output.LastIndexOf("+") + 1);
				return output;
			}
			
			public virtual void LogData (AnalyticsManager analyticsManager)
			{
				analyticsManager.form = new WWWForm();
				analyticsManager.columnData.Clear();
				gameVersion.LogData (analyticsManager);
				player.LogData (analyticsManager);
				scene.LogData (analyticsManager);
				timeSinceLastLog.LogData (analyticsManager);
				eventName.LogData (analyticsManager);
				sessionDuration.LogData (analyticsManager);
				totalGameplayDuration.LogData (analyticsManager);
				sessionNumber.LogData (analyticsManager);
			}
		}

		[Serializable]
		public class PlayerDiedEvent : AnalyticsEvent
		{
			public AnalyticsDataEntry_string killedBy = new AnalyticsDataEntry_string();
			
			public PlayerDiedEvent ()
			{
				Init ();
			}
			
			public override void Init ()
			{
				if (AnalyticsManager.Instance == null)
					return;
				base.Init ();
				PlayerDiedEvent _event = (PlayerDiedEvent) typeof(AnalyticsManager).GetField("_" + GetName()).GetValue(AnalyticsManager.Instance);
				killedBy.dataColumnName = _event.killedBy.dataColumnName;
			}
			
			public override void LogData (AnalyticsManager analyticsManager)
			{
				base.LogData (analyticsManager);
				killedBy.LogData (analyticsManager);
			}
		}
		#endregion
		
		#region Analytics Data Entries
		[Serializable]
		public class AnalyticsDataEntry
		{
			public string dataColumnName;

			public virtual string GetFieldNameInForm (AnalyticsManager analyticsManager)
			{
				return analyticsManager.dataColumnDict[dataColumnName].fieldNameInForm;
			}

			public virtual string GetValue (AnalyticsManager analyticsManager)
			{
				return "";
			}

			public virtual void LogData (AnalyticsManager analyticsManager)
			{
				analyticsManager.form.AddField(GetFieldNameInForm(analyticsManager), GetValue(analyticsManager));
				analyticsManager.columnData.Add(dataColumnName, GetValue(analyticsManager));
			}
		}

		[Serializable]
		public class AnalyticsDataEntry<T> : AnalyticsDataEntry
		{
			public T value;

			public override string GetValue (AnalyticsManager analyticsManager)
			{
				string output = "";
				if (value != null)
					output = value.ToString();
				return output;
			}
		}

		[Serializable]
		public class AnalyticsDataEntry_string : AnalyticsDataEntry<string>
		{
		}

		[Serializable]
		public class AnalyticsDataEntry_float : AnalyticsDataEntry<float>
		{
		}

		[Serializable]
		public class AnalyticsDataEntry_int : AnalyticsDataEntry<int>
		{
		}

		[Serializable]
		public class AnalyticsDataEntry_Vector2 : AnalyticsDataEntry<Vector2>
		{
		}
		#endregion
		
		[Serializable]
		public class GameVersionDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				return "" + BuildManager.Instance.versionIndex;
			}
		}

		[Serializable]
		public class PlayerDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				return "";
			}
		}

		[Serializable]
		public class SceneDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				return SceneManager.GetActiveScene().name;
			}
		}

		[Serializable]
		public class TotalGameplayDurationDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				return "" + (analyticsManager.previousTotalGameplayDuration + analyticsManager.sessionTimer.TimeElapsed);
			}
		}

		[Serializable]
		public class TimeSinceLastLogDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				float timeSinceLastLogged = analyticsManager.timerSinceLastLog.TimeElapsed;
				analyticsManager.timerSinceLastLog.Reset ();
				analyticsManager.timerSinceLastLog.Start ();
				return "" + timeSinceLastLogged;
			}
		}

		[Serializable]
		public class SessionNumberDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				return "" + analyticsManager.sessionNumber;
			}
		}

		[Serializable]
		public class SessionDurationDataEntry : AnalyticsDataEntry
		{
			public override string GetValue (AnalyticsManager analyticsManager)
			{
				return "" + analyticsManager.sessionTimer.TimeElapsed;
			}
		}

		[Serializable]
		public class DataColumn
		{
			public string name;
			public string fieldNameInForm;
		}
	}
}