#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class SetRandomUniqueId : EditorScript
{
	void Awake ()
	{
		foreach (IIdentifiable identifiable in GetComponents<IIdentifiable>())
			identifiable.UniqueId = Random.Range(int.MinValue, int.MaxValue);
		SaveAndLoadObject saveAndLoadObject = GetComponent<SaveAndLoadObject>();
		if (saveAndLoadObject != null)
			saveAndLoadObject.UniqueId = Random.Range(int.MinValue, int.MaxValue);
		DestroyImmediate(this);
	}
}
#endif