using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerIOClient;

namespace ArcherGame
{
	public class NetworkManager : SingletonMonoBehaviour<NetworkManager>
	{
		// public string websiteUri;
		// public Text notificationText;
		public TemporaryActiveText notificationTextObject;
		// public string serverName;
		// public string serverUsername;
		// public string serverPassword;
		// public string databaseName;
		// public const string DEBUG_INDICATOR = "﬩";
		// public static WWWForm defaultDatabaseAccessForm;
		public static Connection connection;
		public static Client client;
		public static bool IsOnline
		{
			get
			{
				return PlayerPrefs.GetInt("Is online", 0) == 1;
				// return false;
			}
			set
			{
				PlayerPrefs.SetInt("Is online", value.GetHashCode());
			}
		}

		public virtual void DisplayNotification (string text)
		{
			StopCoroutine(notificationTextObject.DoRoutine ());
			notificationTextObject.text.text = text;
			StartCoroutine(notificationTextObject.DoRoutine ());
		}

		public virtual void Connect (Callback<Client> onSuccess, Callback<PlayerIOError> onFail)
		{
			PlayerIO.UseSecureApiRequests = true;
			PlayerIO.UnityInit(this);
			PlayerIO.Authenticate("archer-game-2bfjdlweau2y3imzhiljjq",
				"public",
				new Dictionary<string, string> {
					{ "userId", "" },
				},
				null,
				onSuccess,
				onFail
			);
		}

		// public override void Awake ()
		// {
		// 	base.Awake ();
		// 	defaultDatabaseAccessForm = new WWWForm();
		// 	defaultDatabaseAccessForm.AddField("serverName", serverName);
		// 	defaultDatabaseAccessForm.AddField("serverUsername", serverUsername);
		// 	defaultDatabaseAccessForm.AddField("serverPassword", serverPassword);
		// 	defaultDatabaseAccessForm.AddField("databaseName", databaseName);
		// }

		// public virtual IEnumerator PostFormToResource (string resourceName, WWWForm form)
		// {
		// 	using (UnityWebRequest webRequest = UnityWebRequest.Post(websiteUri + "/" + resourceName + "?", form))
		// 	{
		// 		yield return webRequest.SendWebRequest();
		// 		if (webRequest.isHttpError || webRequest.isNetworkError)
		// 		{
		// 			notificationText.text = webRequest.error;
		// 			notificationTextObject.Do ();
		// 			yield return new Exception(notificationText.text);
		// 			yield break;
		// 		}
		// 		else
		// 		{
		// 			yield return webRequest.downloadHandler.text;
		// 			yield break;
		// 		}
		// 		webRequest.Dispose();
		// 	}
		// 	notificationText.text = "Unknown error";
		// 	notificationTextObject.Do ();
		// 	yield return new Exception(notificationText.text);
		// }
	}
}