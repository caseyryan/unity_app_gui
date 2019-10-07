using System;
using UnityEngine;

namespace Components.Clickable {
	
	
	public class HamburgerMenuButton : AnimatableButton {

		[Tooltip("Will apply rotation animation to the inner icon when the drawer it's attached to is opening or closing")]
		[SerializeField] private bool m_RotateButtonOnDrawerOpen = true;
		[Tooltip("Max angle of inner icon rotation in degrees. May be positive or negative. Set negative for clockwise rotation")]
		[SerializeField] private float m_MaxRotationAngle = -180f;

		private float m_RotationValue = 0;
		

		protected override void OnValidate() {
			base.OnValidate();
			m_CurrentColor = ComponentUtils.MixColorsByValue(m_StartColor, m_EndColor, m_RotationValue);
		}

		internal void RotateByValue(float value) {
			
			if (m_RotateButtonOnDrawerOpen) {
				value = Mathf.Clamp01(value);
				m_RotationValue = value;
				var angle = value * m_MaxRotationAngle;
				m_InnerIcon.rectTransform.eulerAngles = new Vector3(0, 0, angle);
				m_CurrentColor = ComponentUtils.MixColorsByValue(m_StartColor, m_EndColor, m_RotationValue);
				SetColors();
			}
			else {
				m_RotationValue = 0;
			}
		}
		protected override void SetAnimationProgress(float value) {
			if (!m_RotateButtonOnDrawerOpen) {
				base.SetAnimationProgress(value);
			}
		}
		
	}
}