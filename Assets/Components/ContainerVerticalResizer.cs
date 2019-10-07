using System.Collections.Generic;
using UnityEngine;

namespace Components {
 
	// changes the height of the container, depending on its children bounding box
	[DisallowMultipleComponent]
	public class ContainerVerticalResizer : MonoBehaviour {
		
		
		private RectTransform m_RectTransform;
		private int m_LastChildCount = 0;
		private float m_MinHeight = 0;
		private readonly List<RectTransform> m_Children = new List<RectTransform>();
		
		
		private void Start() {
			m_RectTransform = GetComponent<RectTransform>();
			SetupMinHeight();
			CalculateContainerSize();
		}

		private void SetupMinHeight() {
			if (m_MinHeight < 1) {
				m_MinHeight = m_RectTransform.rect.height;
			}
		}

		private void OnDisable() {
			m_Children.Clear();
			m_LastChildCount = 0;
		}

		private void CalculateContainerSize() {
			var containerHeight = 0;
			m_Children.ForEach(childRect => {
				var pivotY = childRect.pivot.y;
				var height = childRect.rect.height * pivotY;
				var maxChildY = (int)(Mathf.Abs(childRect.anchoredPosition.y) + height);
				if (maxChildY > containerHeight) {
					containerHeight = maxChildY;
				}
			});
			var size = m_RectTransform.sizeDelta;
			size.y = Mathf.Max(containerHeight, m_MinHeight);
			m_RectTransform.sizeDelta = size;
		}

		private void LateUpdate() {
			var numChildren = m_RectTransform.childCount;
			if (numChildren != m_LastChildCount) {
				m_LastChildCount = numChildren;
				m_Children.Clear();
				for (var i = 0; i < numChildren; i++) {
					var childTransform = m_RectTransform.GetChild(i);
					var childRect = childTransform.GetComponent<RectTransform>();
					if (childRect != null) m_Children.Add(childRect);
				}
			}
			// must be called evety frame to account for possible children positions change
			CalculateContainerSize();
		}
	}
}