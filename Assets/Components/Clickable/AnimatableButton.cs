using Components.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Components.Clickable {
	
	[ExecuteInEditMode]
	public class AnimatableButton : InteractiveObject {

		[SerializeField] protected Image m_Background;
		[SerializeField] protected Text m_Text;
		[Tooltip("Optional icon which could be placed inside this botton and will be colorized if ChangeColors is 'true'")]
		[SerializeField] protected Image m_InnerIcon;
		[SerializeField] protected bool m_IsDisabled = false;
		[SerializeField] private bool m_AnimateColoring = true;
		[Tooltip("Start color for inner contents like icons or texts. This color will be applied on button press")]
		[SerializeField][StartColor] protected Color m_StartColor;
		[Tooltip("End color from inner contents like icons or texts")]
		[SerializeField, EndColor] protected Color m_EndColor;
		[SerializeField, EndColor, Tooltip("The current color of the text and icons inside")] 
		protected Color m_CurrentColor;
		[SerializeField] private Color m_BackgroundColor;
		[SerializeField][DisabledDarkShade] private Color m_DisabledColor;
		[SerializeField] private float m_AnimationSpeed = 30.0f;
		private float m_CurAnimationProgress = 1.0f;


		private void Start() {
			
		}

		protected override void OnValidate() {
			base.OnValidate();
			m_CurrentColor = m_EndColor;
			SetColors();
		}

		protected override void SetColors() {
			if (!m_ChangeColor) return;
			
			if (m_Background != null) {
				m_Background.color = m_BackgroundColor;
			}
			if (m_InnerIcon != null) {
				m_InnerIcon.color = m_CurrentColor;
			}

			if (m_Text != null) {
				m_Text.color = m_CurrentColor;
			}

			
		}

		private void FixedUpdate() {
			if (m_IsDisabled || !m_AnimateColoring) return;
			if (!m_IsPressed) {
				if (m_CurAnimationProgress > .9990f) return;
				m_CurAnimationProgress += m_AnimationSpeed * Time.deltaTime;
			}
			else {
				if (m_CurAnimationProgress < .0001f) return;
				m_CurAnimationProgress -= m_AnimationSpeed * Time.deltaTime;
			}
			m_CurAnimationProgress = Mathf.Clamp01(m_CurAnimationProgress);
			SetAnimationProgress(m_CurAnimationProgress);
		}
		
		/// <summary>
		/// Can be used in the inheritors
		/// </summary>
		/// <param name="value"></param>
		protected virtual void SetAnimationProgress(float value) {
			m_CurrentColor = ComponentUtils.MixColorsByValue(m_StartColor, m_EndColor, value);
			SetColors();
		}
		
		public override void OnPointerDown(PointerEventData eventData) {
			if (m_IsDisabled) return;
			base.OnPointerDown(eventData);
			if (!m_AnimateColoring) {
				m_CurrentColor = m_EndColor;
				SetColors();
			}
			
		}

		public override void OnPointerUp(PointerEventData eventData) {
			if (m_IsDisabled) return;
			base.OnPointerUp(eventData);
			if (!m_AnimateColoring) {
				m_CurrentColor = m_StartColor;
				SetColors();
			}
		}
	}
}