using UnityEngine;
using Extensions;

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class ObjectInWorld : MonoBehaviour
	{
		public bool IsInPieces
		{
			get
			{
				return pieceIAmIn != null;
			}
		}
		public Transform trs;
		public GameObject go;
		public Transform duplicateTrs;
		public GameObject duplicateGo;
		public ObjectInWorld duplicateWorldObject;
		public WorldPiece pieceIAmIn;
		public GameObject[] gosWithComponents = new GameObject[0];

#if UNITY_EDITOR
		public virtual void OnEnable ()
		{
			if (Application.isPlaying)
				return;
			if (trs == null)
				trs = GetComponent<Transform>();
			if (go == null)
				go = gameObject;
			if (duplicateTrs == null)
			{
				GameObject gameSceneRootGo = GameObject.Find("Game Scene");
				if (gameSceneRootGo != null)
				{
					Transform gameSceneRoot = gameSceneRootGo.GetComponent<Transform>();
					if (gameSceneRoot != null)
					{
						foreach (Transform child in gameSceneRoot.FindChildren(name))
						{
							ObjectInWorld objectInWorld = child.GetComponent<ObjectInWorld>();
							if (objectInWorld != null && objectInWorld.IsInPieces != IsInPieces && child.position == trs.position)
							{
								objectInWorld.duplicateTrs = trs;
								objectInWorld.duplicateGo = go;
								duplicateTrs = objectInWorld.trs;
								duplicateGo = objectInWorld.go;
							}
						}
					}
				}
			}
			if (gosWithComponents.Length == 0)
				gosWithComponents = new GameObject[1] { go };
		}
#endif

		public virtual void OnDisable ()
		{
		}
	}
}