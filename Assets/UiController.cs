using System;
using Components;
using Components.ActionBar;
using Components.Drawer;
using UnityEditor;
using UnityEngine;


//public class ComponentHolderComparer : IComparer<ComponentHolder> {
//	
//	public int Compare(ComponentHolder x, ComponentHolder y) {
//		if (x == null || y == null || (x.m_ComponentPriority == y.m_ComponentPriority)) return 0;
//		if (x.m_ComponentPriority > y.m_ComponentPriority) return 1;
//		return -1;
//	}
//}

[ExecuteInEditMode]
public class UiController : ComponentBehaviour {

	/// <summary>
	/// Класс является синглтоном. Вызывается через UIController.Instance
	/// этот компонент ДОЛЖЕН быть зацеплен к канвасу, в котором расположены все
	/// остальные компоненты системы, такие как ящики, навбары, экшнбары, окна и прочая хуета
	/// Контролирует глубину расположения других компонентов за счет их свойства ComponentPriority
	/// Так же с помощью объекта этого класса можно управлять некоторыми другими элементами UI
	/// например спрятать или показать ActionBar, показать попап и др.
	/// Если нужно управлять компонентами, не надо дописывать код сюда. Просто создавай свой класс и цепляй его
	/// к тому же канвасу, и там уже делай все, что угодно, товарищ :)
	/// </summary>


	private static UiController m_Instance;

	public static UiController Instance {
		get { return m_Instance; }
	}
	
	// all this stuff might be unavailable if not added via Editor Menu
	private ActionBar m_ActionBar;
	private NavigationDrawerPanel m_NavigationDrawerPanel;
	
	

	private void Start() {
		if (m_Instance != null) {
			Destroy(gameObject);
			return;
		}
		m_Instance = this;
		m_ActionBar = GetSingleComponent<ActionBar>();
		
		
		
		// Just to let all components initialize before sorting
//		DelayCall(Sort, .15f);
	}

	public NavigationDrawerPanel NavigationDrawerPanel {
		get {
			if (m_NavigationDrawerPanel != null) {
				return m_NavigationDrawerPanel;
			}
			m_NavigationDrawerPanel = GetComponentInChildren<NavigationDrawerPanel>(true);
			return m_NavigationDrawerPanel;
		}
	}
	public ActionBar ActionBar {
		get {
			if (m_ActionBar != null) {
				return m_ActionBar;
			}
			m_ActionBar = GetComponentInChildren<ActionBar>(true);
			return m_ActionBar;
		}
	}
	
	public void AddNavigationDrawerPanel() {
#if UNITY_EDITOR
		if (NavigationDrawerPanel == null && !Application.isPlaying) {
			m_NavigationDrawerPanel = ComponentUtils.InstantiatePrefab("NavigationDrawerPanel_pfb")?.GetComponent<NavigationDrawerPanel>();
			if (m_NavigationDrawerPanel != null) {
				m_NavigationDrawerPanel.transform.SetParent(GetComponent<RectTransform>(), false);
			}
			EditorUtility.DisplayDialog("Information", "Navigation Drawer panel has been successfully added!", "Ok");
			return;
		}
		EditorUtility.DisplayDialog("Information", "You only need one drawer panel per application.\nAdding more is useless.", "Ok");
#endif
	}
	public void AddActionBar() {
#if UNITY_EDITOR
		if (ActionBar == null && !Application.isPlaying) {
			m_ActionBar = ComponentUtils.InstantiatePrefab("ActionBar_pfb")?.GetComponent<ActionBar>();
			if (m_ActionBar != null) {
				m_ActionBar.transform.SetParent(GetComponent<RectTransform>(), false);
			}
			EditorUtility.DisplayDialog("Information", "ActionBar has been successfully added!", "Ok");
			return;
		}
		EditorUtility.DisplayDialog("Information", "You only need one action bar per application.\nAdding more is useless.", "Ok");
#endif
	}
	
	
	/// <summary>
	/// Must be called in edit mode from NiceUI Kit -> Remove Drawer Panel menu
	/// </summary>
	public void RemoveNavigationDrawerPanel() {
#if UNITY_EDITOR
		if (NavigationDrawerPanel != null && !Application.isPlaying) {
			DestroyImmediate(NavigationDrawerPanel.gameObject);
			m_NavigationDrawerPanel = null;
		}
#endif
	}
	public void RemoveActionBar() {
#if UNITY_EDITOR
		if (ActionBar != null && !Application.isPlaying) {
			DestroyImmediate(ActionBar.gameObject);
			m_ActionBar = null;
		}
#endif
	}
	
	private T GetSingleComponent<T>() {
		var componentsInChildren = transform.GetComponentsInChildren<T>();
		if (componentsInChildren.Length > 1) {
			throw new Exception($"Компонентов с типом {typeof(T)} " +
			                    $"должно быть не больше 1. " +
			                    $"Найдено {componentsInChildren.Length}");
		}
		return componentsInChildren.Length > 0 ? componentsInChildren[0] : default(T);
	}

//	private void Sort() {
//		IComparer<ComponentHolder> comparer = new ComponentHolderComparer();
//		var componentHolders = GetComponentsInChildren<ComponentHolder>(true);
//		Array.Sort(componentHolders, comparer);	
//		foreach (var componentHolder in componentHolders) {
//			if (componentHolder.transform.parent == transform) {
//				// нужно учитывать только прямых потомков канваса
//				componentHolder.transform.SetAsLastSibling();
//			}
//		}
//	}

	public void ShowActionBar() {
		if (m_ActionBar != null) m_ActionBar.HideActionBar();
	}

	public void HideActionBar() {
		if (m_ActionBar != null) m_ActionBar.HideActionBar();
	}


}
