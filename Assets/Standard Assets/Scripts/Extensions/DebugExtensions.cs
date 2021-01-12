using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArcherGame;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Extensions
{
	public static class DebugExtensions
	{
		public static void DrawPoint (Vector3 point, float radius, Color color, float duration)
		{
			Debug.DrawLine(point + (Vector3.right * radius), point + (Vector3.left * radius), color, duration);
			Debug.DrawLine(point + (Vector3.up * radius), point + (Vector3.down * radius), color, duration);
			Debug.DrawLine(point + (Vector3.forward * radius), point + (Vector3.back * radius), color, duration);
		}

		public static void DrawRect (Rect rect, Color color, float duration)
		{
			Debug.DrawLine(rect.min, new Vector2(rect.xMin, rect.yMax), color, duration);
			Debug.DrawLine(new Vector2(rect.xMin, rect.yMax), rect.max, color, duration);
			Debug.DrawLine(rect.max, new Vector2(rect.xMax, rect.yMin), color, duration);
			Debug.DrawLine(new Vector2(rect.xMax, rect.yMin), rect.min, color, duration);
		}

		// public static void DrawText (string text)
		// {
		// }

		public static void Log (string elementSeperator = ", ", LogType logType = LogType.Info, params object[] data)
		{
			if (logType == LogType.Info)
				Debug.Log(data.ToString(elementSeperator));
			else if (logType == LogType.Warning)
				Debug.LogWarning(data.ToString(elementSeperator));
			else
				Debug.LogError(data.ToString(elementSeperator));
		}

		public static void Log<T> (string elementSeperator = ", ", LogType logType = LogType.Info, params Data<T>[] data)
		{
			string output = "";
			foreach (Data<T> dataPiece in data)
				output += dataPiece.value.ToString() + elementSeperator;
			if (logType == LogType.Info)
				Debug.Log(output);
			else if (logType == LogType.Warning)
				Debug.LogWarning(output);
			else
				Debug.LogError(output);
		}

		public static void Log (in object data, LogType logType = LogType.Info)
		{
			if (logType == LogType.Info)
				Debug.Log(data.ToString());
			else if (logType == LogType.Warning)
				Debug.LogWarning(data.ToString());
			else
				Debug.LogError(data.ToString());
		}

		public static void DelayedLog (float delay, bool realtime = true, string elementSeperator = ", ", LogType logType = LogType.Info, params object[] data)
		{
			GameManager.Instance.StartCoroutine(DelayedLogRoutine (delay, realtime, elementSeperator, logType, data));
		}

		public static IEnumerator DelayedLogRoutine (float delay, bool realtime = true, string elementSeperator = ", ", LogType logType = LogType.Info, params object[] data)
		{
			if (realtime)
				yield return new WaitForSecondsRealtime(delay);
			else
				yield return new WaitForSeconds(delay);
			DebugExtensions.Log (elementSeperator, logType, data);
		}

		public static void DelayedLog<T> (float delay, bool realtime = true, string elementSeperator = ", ", LogType logType = LogType.Info, params Data<T>[] data)
		{
			GameManager.Instance.StartCoroutine(DelayedLogRoutine (delay, realtime, elementSeperator, logType, data));
		}

		public static IEnumerator DelayedLogRoutine<T> (float delay, bool realtime = true, string elementSeperator = ", ", LogType logType = LogType.Info, params Data<T>[] data)
		{
			if (realtime)
				yield return new WaitForSecondsRealtime(delay);
			else
				yield return new WaitForSeconds(delay);
			DebugExtensions.Log (elementSeperator, logType, data);
		}

		public enum LogType
		{
			Info,
			Error,
			Warning
		}
	}
}