using System;
using System.Collections.Generic;
using System.IO;
using Components.Texts;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Components {
	public static class ComponentUtils {


		/// <summary>
		/// Mixes 2 colors by a specified value
		/// </summary>
		/// <param name="startColor"></param>
		/// <param name="endColor"></param>
		/// <param name="mixValue">a value of mixture from 0 to 1</param>
		/// <returns>A resulting color</returns>
		public static Color MixColorsByValue(Color startColor, Color endColor, float mixValue) {
			mixValue = Mathf.Clamp01(mixValue);
			return startColor + ((endColor - startColor) * mixValue);
		}
		
		/// <summary>
		/// Finds all object of a specified type in child transforms and adds them to List
		/// </summary>
		/// <param name="transform">The parent transform</param>
		/// <param name="toCollection">a collection to use as a storage</param>
		/// <typeparam name="T"></typeparam>
		public static void CollectInstancesInChildren<T>(Transform transform, List<T> toCollection) {
			var numChildren = transform.childCount;
			if (numChildren > 0) {
				for (var i = 0; i < numChildren; i++) {
					var child = transform.GetChild(i);
					CollectInstancesInChildren(child, toCollection);
				}
			}
			var instance = transform.gameObject.GetComponent<T>();
			if (instance != null) {
				toCollection.Add(instance);
			}
		}
		
		public static GameObject InstantiatePrefab(string prefabName) {
			return (GameObject)Object.Instantiate(Resources.Load(Path.Combine("NiceUIPrefabs", prefabName)));
		}
		
		public static string GetProjectName() {
			var splitPath = Application.dataPath.Split('/');
			var projectName = splitPath[splitPath.Length - 2];
			return projectName;
		}
		/// <summary>
		/// Finds all objects that have attached component of the specified type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindObjectsWithComponentsOfType<T>(bool onlyActiveInHierarchy = false) where T : Behaviour {
			var allGameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			var result = new List<T>();
			foreach (var go in allGameObjects) {
				var componentHolder = go.GetComponent<T>();
				if (onlyActiveInHierarchy) {
					if (componentHolder == null || !componentHolder.gameObject.activeInHierarchy) {
						continue;
					}
					result.Add(componentHolder);
				}
				else {
					if (componentHolder != null) {
						result.Add(componentHolder);
					}
				}
			}
			return result;
		}
		
	}
}