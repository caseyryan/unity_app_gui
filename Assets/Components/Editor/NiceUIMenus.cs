using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Debug;

namespace Components.Editor {
	public class NiceUiMenus {

		private static UiController m_UiController;
		
		[MenuItem("NiceUI Kit/Initialize Application Frame", false, -10)]
		private static void InitializeAppFrame() {
			if (IsInitialized()) {
				EditorUtility.DisplayDialog("Information", "Already initialized!", "Ok");
				return;
			}
			InitUiController();
		}

		[MenuItem("NiceUI Kit/Add Navigation Drawer Panel")]
		private static void AddNavigationDrawerPanel() {
			GetUiController()?.AddNavigationDrawerPanel();
		}
		[MenuItem("NiceUI Kit/Add Action Bar")]
		private static void AddActionBar() {
			GetUiController()?.AddActionBar();
		}
		
		
		[MenuItem("NiceUI Kit/Remove Navigation Drawer Panel")]
		private static void RemoveDrawerPanel() {
			if (!IsInitialized()) {
				EditorUtility.DisplayDialog("Warning", "Nothing to remove", "Ok");
				return;
			}

			if (EditorUtility.DisplayDialog("Warning", 
				"The panel with drawers will be completely destroyed!\nIt is not possible to undo this operation!\n" +
				"Are you sure you want to continue and remove it?", "Remove", "Cancel")) {
				m_UiController.RemoveNavigationDrawerPanel();
			}
		}
		[MenuItem("NiceUI Kit/Remove Action Bar")]
		private static void RemoveActionBar() {
			if (!IsInitialized()) {
				EditorUtility.DisplayDialog("Warning", "Nothing to remove", "Ok");
				return;
			}
			if (EditorUtility.DisplayDialog("Warning", 
				"The action bar will be completely destroyed!\nIt is not possible to undo this operation!\n" +
				"Are you sure you want to continue and remove it?", "Remove", "Cancel")) {
				m_UiController.RemoveActionBar();
			}
		}
		

		private static bool IsInitialized() {
			var allGameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			var uiHolderGameObject = allGameObjects.FirstOrDefault(g => {
				var uiController = g.GetComponent<UiController>();
				return uiController != null && g.activeInHierarchy;
			});
			if (uiHolderGameObject != null && m_UiController == null) {
				m_UiController = uiHolderGameObject.GetComponent<UiController>();
			}
			
			return uiHolderGameObject != null && uiHolderGameObject.activeInHierarchy;
		}


		private static UiController GetUiController() {
			if (m_UiController != null) return m_UiController;
			// если при загрузке юньки уже есть контроллер, но ссылки еще нет
			var allGameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach (var gameObject in allGameObjects) {
				var uiController = gameObject.GetComponent<UiController>();
				if (uiController == null || !gameObject.activeInHierarchy) continue;
				m_UiController = uiController;
				break;
			}

			if (m_UiController == null) {
				if (EditorUtility.DisplayDialog("Warning",
					"The application needs to be initialized first. Do you want to initialize in now?", "Yes", "No")) {
					InitializeAppFrame();
				}
			}
			
			return m_UiController;
		}
		
		private static void InitUiController() {
			if (IsInitialized()) return;
			m_UiController = ComponentUtils.InstantiatePrefab("UIController_pfb")?.GetComponent<UiController>();
		} 
	}
}