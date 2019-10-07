using System;
using System.Collections;
using UnityEngine;

namespace Components {
	public class ComponentBehaviour : MonoBehaviour {


		[Tooltip("Whether or not will this component use theme colors which you set in NiceUI Kit Theme Editor window")]
		[SerializeField] private bool UseGlobalThemeColors = true;
		[Tooltip("If 'false' the selected colors won't affect the component")]
		[SerializeField] internal bool m_ChangeColor = false;
		//[SerializeField] internal float m_AnimationSpeed = 30.0f;
		private RectTransform m_RectTransform;

		/// <summary>
		/// Calls a method after some delay.
		/// Might be useful in an asynchronous scenario
		/// </summary>
		/// <param name="callback">A delegate to be called after the delay</param>
		/// <param name="delay">Delay in seconds</param>
		public void DelayCall(Action callback, float delay) {
			if (callback != null) {
				StartCoroutine(Caller(callback, delay));
			}
		}

		protected RectTransform GetRectTransform() {
			if (m_RectTransform == null) {
				m_RectTransform = GetComponent<RectTransform>();
			}

			return m_RectTransform;
		}
		
		protected virtual void OnValidate() {}

		private static IEnumerator Caller(Action callback, float delay) {
			yield return new WaitForSeconds(delay);
			callback?.Invoke();
		}
		protected virtual void SetColors() {}
		
	}
}