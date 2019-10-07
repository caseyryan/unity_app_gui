using UnityEngine;

namespace Components.Pages {
	
	public class ParallaxScrollPage : PageBase {

		[SerializeField, Tooltip("The container which will may contain header image or some" +
		" texts, logos etc. and is scrolling slower than the rest of the page")]
		private RectTransform m_TopContainer;
		[Tooltip("The actual scroll container. Put all your page contents inside it")]
		[SerializeField] private RectTransform m_ScrollContainer;
		[Tooltip("The smaller this value is the slower will the parallax effect be")]
		[SerializeField] private float m_ParallaxCoef = .5f;
		private Vector2 m_InitialScrollPosition;
		private QuickScrollerVertical m_QuickScrollerVertical;
		

		private void Start() {
			OnValidate();
		}
		

		private QuickScrollerVertical GetScroller() {
			if (m_QuickScrollerVertical == null) {
				m_QuickScrollerVertical = GetComponent<QuickScrollerVertical>();
			}
			return m_QuickScrollerVertical;
		}
		
		protected override void OnValidate() {
			m_ParallaxCoef = Mathf.Clamp01(m_ParallaxCoef);
			m_InitialScrollPosition = m_ScrollContainer.anchoredPosition;
			CalculateParallax();
		}

		private void FixedUpdate() {
			CalculateParallax();
			if (NavigationDrawer != null) {
				// this can happen only if the page was added to a NavigationScroller's container
				GetScroller().IsEnabled = NavigationDrawer.IsScrollAllowed;
			}
		}

		private void CalculateParallax() {
			var pos = m_ScrollContainer.anchoredPosition;
			pos.y = Mathf.Clamp(pos.y, m_InitialScrollPosition.y, 0);
			var rect = m_TopContainer.rect;
			var parallaxAmount = (rect.height + pos.y) / rect.height;
			MoveTopContainer(parallaxAmount);
		}

		private void MoveTopContainer(float parallaxAmount) {
			var pos = m_TopContainer.anchoredPosition;
			pos.y = (parallaxAmount * m_ParallaxCoef) * m_TopContainer.rect.height;
			m_TopContainer.anchoredPosition = pos;
		}
	}
}