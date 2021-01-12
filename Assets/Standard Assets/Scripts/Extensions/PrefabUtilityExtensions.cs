#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using ArcherGame;

namespace Extensions
{
	public static class PrefabUtilityExtensions
	{
		public static GameObject ClonePrefabInstance (GameObject prefabInstanceGo)
		{
			List<AddedGameObject> addedGos = PrefabUtility.GetAddedGameObjects(prefabInstanceGo);
			List<AddedComponent> addedComponents = PrefabUtility.GetAddedComponents(prefabInstanceGo);
			List<RemovedComponent> removedComponents = PrefabUtility.GetRemovedComponents(prefabInstanceGo);
			PropertyModification[] propertyModifications = PrefabUtility.GetPropertyModifications(prefabInstanceGo);
			GameObject output = (GameObject) PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(prefabInstanceGo));
			Transform prefabInstanceTrs = prefabInstanceGo.GetComponent<Transform>();
			Transform addedTrs;
			Transform clonedTrs;
			foreach (AddedGameObject addedGo in addedGos)
			{
				addedTrs = addedGo.instanceGameObject.GetComponent<Transform>();
				GameObject addedGoClone = (GameObject) GameManager.Clone (addedGo.instanceGameObject, TransformExtensions.FindEquivalentChild(prefabInstanceTrs, addedTrs, output.GetComponent<Transform>()));
				clonedTrs = addedGoClone.GetComponent<Transform>();
				clonedTrs.position = addedTrs.position;
				clonedTrs.rotation = addedTrs.rotation;
				clonedTrs.localScale = addedTrs.localScale;
			}
			foreach (AddedComponent addedComponent in addedComponents)
				output.AddComponent(addedComponent.instanceComponent.GetType());
			foreach (RemovedComponent removedComponent in removedComponents)
				GameManager._DestroyImmediate(removedComponent.containingInstanceGameObject.GetComponent(removedComponent.assetComponent.GetType()));
			PropertyModification propertyModification;
			for (int i = 0; i < propertyModifications.Length; i ++)
			{
				propertyModification = propertyModifications[i];
				if (propertyModification.objectReference == null || propertyModification.target == null)
				{
					propertyModifications = propertyModifications.RemoveAt(i);
					i --;
				}
			}
			if (propertyModifications.Length > 0)
				PrefabUtility.SetPropertyModifications(output, propertyModifications);
			output.name = prefabInstanceGo.name;
			output.layer = prefabInstanceGo.layer;
			return output;
		}
	}
}
#endif