using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Components {
	public class InteractiveObject : ComponentBehaviour, IPointerClickHandler,
		IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

		protected bool m_IsPressed = false;

		/// <summary>
		/// usage example:
		/// 
		/// someObject.Click += ClickHandler;
		/// 
		/// private ClickHandler(InteractiveObject target, PointerEventData eventData) {
		/// 	print($"Clicked object {target}");
		/// };
		/// 
		/// don't forget to remove listeners when they are not needed anymore
		/// 
		/// someObject.Click -= ClickHandler;
		/// </summary>
		
		public event UnityAction<InteractiveObject, PointerEventData> Click;
		public event UnityAction<InteractiveObject, PointerEventData> PointerDown;
		public event UnityAction<InteractiveObject, PointerEventData> PointerUp;
		public event UnityAction<InteractiveObject, PointerEventData> PointerEnter;
		public event UnityAction<InteractiveObject, PointerEventData> PointerExit;
		/// <summary>
		/// Dispatched when parent changes. Passes this interactive object and new parent
		/// </summary>
		public event UnityAction<InteractiveObject, Transform> ParentChanged;

		private Transform m_Parent;
		// used to detect double tap, because eventData.clickCount doesn't work with taps on android
		private float m_LastTapTime = 0;
		private float m_DoubleClickDelay = .35f;
		private int m_TapCount = 0;

		private void Update() {
			if (transform.parent != m_Parent) {
				m_Parent = transform.parent;
				ParentChanged?.Invoke(this, m_Parent);
			}
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
			Click?.Invoke(this, eventData);
			var curTime = Time.time;
			if (curTime - m_LastTapTime < m_DoubleClickDelay) {
				m_TapCount++;
			}
			else m_TapCount = 1;
			m_LastTapTime = curTime;
			if (m_TapCount == 2) {
				OnPointerDoubleClick(eventData);
			}
			ForwardToParents<IPointerClickHandler>((parent) => parent.OnPointerClick(eventData));
		}

		public virtual void OnPointerDoubleClick(PointerEventData eventData) {
			
		}

		public virtual void OnPointerDown(PointerEventData eventData) {
			m_IsPressed = true;
			PointerDown?.Invoke(this, eventData);
			ForwardToParents<IPointerDownHandler>((parent) => parent.OnPointerDown(eventData));
		}

		public virtual void OnPointerUp(PointerEventData eventData) {
			m_IsPressed = false;
			PointerUp?.Invoke(this, eventData);
			ForwardToParents<IPointerUpHandler>((parent) => parent.OnPointerUp(eventData));
		}

		public virtual void OnPointerEnter(PointerEventData eventData) {
			PointerEnter?.Invoke(this, eventData);
			ForwardToParents<IPointerEnterHandler>((parent) => parent.OnPointerEnter(eventData));
		}

		public virtual void OnPointerExit(PointerEventData eventData) {
			PointerExit?.Invoke(this, eventData);
			ForwardToParents<IPointerExitHandler>((parent) => parent.OnPointerExit(eventData));
		}

		public bool Active {
			get { return gameObject.activeInHierarchy; }
			set { gameObject.SetActive(value); }
		}
		
		protected void ForwardToParents<T>(Action<T> action) where T : IEventSystemHandler {
			var parent = transform.parent;
			
			while (parent != null) {
				foreach (var component in parent.GetComponents<Component>()) {
					if (component is T) {
						action((T) (IEventSystemHandler) component);
					}
				}
				parent = parent.parent;
			}
		}
	}
}
