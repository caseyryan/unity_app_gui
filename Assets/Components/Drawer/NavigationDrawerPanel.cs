using System;
using System.Linq;
using UnityEngine;

namespace Components.Drawer {
	
	// a window which simply holds 2 or 1 drawer
	[ExecuteInEditMode]
	public class NavigationDrawerPanel : ComponentHolder {


		private NavigationDrawer[] m_NavigationDrawers;
		[SerializeField] private bool m_RightDrawerActive = true;
		[SerializeField] private bool m_LeftDrawerActive = true;
		
		private void Start() {
			AttachDrawers();
		}
		
		private void AttachDrawers() {
			m_NavigationDrawers = transform.GetComponentsInChildren<NavigationDrawer>(true);
			if (m_NavigationDrawers.Length > 2) {
				throw new Exception("There can be no more than 2 drawers!");
			}
			
			var numLeft = m_NavigationDrawers.Count(drawer => drawer.NavigationDrawerPosition == NavigationDrawerPosition.Left);
			var numRight = m_NavigationDrawers.Count(drawer => drawer.NavigationDrawerPosition == NavigationDrawerPosition.Right);
			if (numLeft > 1 || numRight > 1) {
				throw new Exception("Only 1 drawer for each position is allowed!");
			}
			var leftDrawer = GetDrawerForPosition(NavigationDrawerPosition.Left);
			if (leftDrawer != null) leftDrawer.gameObject.SetActive(m_LeftDrawerActive);
				
			var rightDrawer = GetDrawerForPosition(NavigationDrawerPosition.Right);
			if (rightDrawer != null) rightDrawer.gameObject.SetActive(m_RightDrawerActive);
		}
		
		public NavigationDrawer GetDrawerForPosition(NavigationDrawerPosition navigationDrawerPosition) {
			if (m_NavigationDrawers == null) {
				AttachDrawers();
			}
			return m_NavigationDrawers.FirstOrDefault(d => d.NavigationDrawerPosition == navigationDrawerPosition);
		}

		protected override void OnValidate() {
			AttachDrawers();
			
		}
	}
}