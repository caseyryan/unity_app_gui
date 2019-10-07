using Components.Attributes;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Components.ActionBar {
	
	[DisallowMultipleComponent]
	public class ActionBar : ComponentHolder {

		[Tooltip("Если true, то при прохождении сообщения ActionBarOff, уберется за экран, а ActionBarOn вернет его")]
		[SerializeField] private bool m_CanDisappear = true;
		[SerializeField] private float m_HideAnimationTime = .2f;
		
		[SerializeField] 
		private Image m_Background;
		
		private void Awake() {
			
			Height = GetRectTransform().rect.height;
		}

		public float Height { get; private set; }

		/// <summary>
		/// Эти методы вызываются через UIController.Instance.HideActionBar()
		/// UIController.Instance.ShowActionBar()
		/// </summary>
		internal void HideActionBar() {
			if (!m_CanDisappear) {
				print($"ActionBar невозможно скрыть! Свойство CanDisappear = false; {this}");
				return;
			}
			GetRectTransform().DOAnchorPosY(Height, m_HideAnimationTime);
		}

		internal void ShowActionBar() {
			if (!m_CanDisappear) {
				print($"ActionBar невозможно скрыть! Свойство CanDisappear = false; {this}");
				return;
			}
			GetRectTransform().DOAnchorPosY(0, m_HideAnimationTime);
		}
	}
}