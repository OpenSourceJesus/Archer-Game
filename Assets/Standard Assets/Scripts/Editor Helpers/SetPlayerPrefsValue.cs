#if UNITY_EDITOR
using UnityEngine;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class SetPlayerPrefsValue : MonoBehaviour
	{
		public bool update;
        public string key;
		public PlayerPrefsType type;
        public int intValue;
        public float floatValue;
        public string stringValue;

		public virtual void Update ()
		{
			if (!update)
				return;
			update = false;
            if (type == PlayerPrefsType.Int)
                PlayerPrefs.SetInt(key, intValue);
            else if (type == PlayerPrefsType.Float)
                PlayerPrefs.SetFloat(key, floatValue);
            else// if (type == PlayerPrefsType.String)
                PlayerPrefs.SetString(key, stringValue);
			DestroyImmediate(this);
		}

        public enum PlayerPrefsType
        {
            Int,
            Float,
            String
        }
	}
}
#endif