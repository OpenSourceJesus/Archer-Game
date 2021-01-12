using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class ZOrderManager : SingletonMonoBehaviour<ZOrderManager>
	{
		public GameObject[] addToZOrderedObjects = new GameObject[0];
		public List<ZOrderedObject> zOrderedObjects = new List<ZOrderedObject>();
		public List<Renderer> renderers = new List<Renderer>();
		public const int MIN_Z_ORDER = -32768;
		public const int MAX_Z_ORDER = 32767;

		[Serializable]
		public class ZOrderedObject
		{
			public string objectName;
			public int index;
			public Renderer renderer;
			[Range(MIN_Z_ORDER, MAX_Z_ORDER)]
			public int order;
			public string layerName;
			public int layerIndex;
		}

		public virtual void OnValidate ()
		{
			ZOrderedObject zOrderedObject;
			Renderer renderer;
			foreach (GameObject obj in addToZOrderedObjects)
			{
				renderer = obj.GetComponentInChildren<Renderer>();
				zOrderedObject = new ZOrderedObject();
				zOrderedObject.objectName = obj.name;
				zOrderedObject.order = renderer.sortingOrder;
				zOrderedObject.layerName = renderer.sortingLayerName;
				zOrderedObject.layerIndex = renderer.sortingLayerID;
				zOrderedObject.renderer = renderer;
				zOrderedObject.index = int.MaxValue;
				renderers.Add(renderer);
				renderers.Sort((a, b) => RendererSorter(a, b));
				zOrderedObjects.Insert(renderers.IndexOf(renderer), zOrderedObject);
			}
			addToZOrderedObjects = new GameObject[0];
			int previousLayerIndex = int.MinValue;
			int previousOrder = MIN_Z_ORDER;
			for (int i = 0; i < zOrderedObjects.Count; i ++)
			{
				zOrderedObject = zOrderedObjects[i];
				if (zOrderedObject.layerIndex > previousLayerIndex)
				{
					previousLayerIndex = zOrderedObject.layerIndex;
					previousOrder = MIN_Z_ORDER;
				}
				else
					previousOrder ++;
				zOrderedObject.index = i;
				zOrderedObject.order = previousOrder;
				zOrderedObjects[i] = zOrderedObject;
			}
			zOrderedObjects.Sort((a, b) => ZOrderedObjectSorter(a, b));
		}

		public virtual int RendererSorter (Renderer r1, Renderer r2)
		{
			int output = 0;
			if (r1.sortingLayerID > r2.sortingLayerID || r1.sortingOrder > r2.sortingOrder)
				output = 1;
			else if (r2.sortingLayerID > r1.sortingLayerID || r2.sortingOrder > r1.sortingOrder)
				output = -1;
			return output;
		}

		public virtual int ZOrderedObjectSorter (ZOrderedObject o1, ZOrderedObject o2)
		{
			int output = 0;
			if (o1.layerIndex > o2.layerIndex || o1.index > o2.index)
				output = 1;
			else if (o2.layerIndex > o1.layerIndex || o2.index > o1.index)
				output = -1;
			return output;
		}

		public virtual void ApplyChanges ()
		{
			foreach (ZOrderedObject zOrderedObject in zOrderedObjects)
			{
				zOrderedObject.renderer.sortingOrder = zOrderedObject.order;
				zOrderedObject.renderer.sortingLayerName = zOrderedObject.layerName;
			}
		}

#if UNITY_EDITOR
		[MenuItem("Z Order/Apply Changes")]
		public static void _ApplyChanges ()
		{
			ZOrderManager.Instance.ApplyChanges ();
		}
#endif
	}
}