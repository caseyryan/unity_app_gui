using UnityEngine;

namespace Components {
	
	public class HorizontalProgressBar : ProgressBar {

		private float m_Width;
		
		protected override void OnValidate() {
			m_Width = RectTransform.rect.width;
			base.OnValidate();
		}

		protected override void SetFillAmount(float amount) {
			var sizeDelta = ProgressTransform.sizeDelta;
			ProgressTransform.sizeDelta = new Vector2(m_Width * amount, sizeDelta.y);
		}
	}
}