using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Components.Clickable {
	
	[ExecuteInEditMode]
	public class Toggle : InteractiveObject {
			
		[Tooltip("Whether it's nevessary or not to tween color from one color to another when the toggle is switched")]
		[SerializeField] private bool m_ChangeColorOnSwitch;
		[SerializeField] private Image m_Background;
		[SerializeField] private Image m_Knob;
		[SerializeField] private Color m_KnobOffColor;
		[SerializeField] private Color m_KnobOnColor;	
		[SerializeField] private Color m_BackgroundOffColor;
		[SerializeField] private Color m_BackgroundOnColor;
		[SerializeField] private Color m_BackgroundDisabledColor;
		[SerializeField] private Color m_KnobDisabledColor;
		[SerializeField] private bool m_IsOn = false;
		[SerializeField] private bool m_IsDisabled = false;
		[SerializeField] private bool m_AnimateToggling = true;
		// a value between 0 and 1
		private float m_CurMoveX = 0;
		private RectTransform m_KnobTransform;
		private float m_KnobPosY = 0;
		private float m_StartX = 0;
		private float m_EndX = 0;

		public event UnityAction<bool> ValueChange; 
		
		private void Start() {
			Setup();
		}
		
		public bool IsOn {
			get { return m_IsOn; }
			set {
				if (IsDisabled) return;
				
				if (m_IsOn != value) {
					m_IsOn = value;
					if (!m_AnimateToggling) {
						ChangeView(value ? 1 : 0);
					}
					ValueChange?.Invoke(value);
				}
				
			}
		}

		public bool IsDisabled {
			get { return m_IsDisabled; }
			set {
				m_IsDisabled = value;
				if (value) {
					m_Background.color = m_BackgroundDisabledColor;
					m_Knob.color = m_KnobDisabledColor;
				}
				else {
					ChangeView(IsOn ? 1 : 0);
				}
			}
		}

		private void ChangeView(float knobMovePercent) {
			if (m_ChangeColorOnSwitch) {
				m_Knob.color = ComponentUtils.MixColorsByValue(m_KnobOffColor, m_KnobOnColor, knobMovePercent);
				m_Background.color = ComponentUtils.MixColorsByValue(m_BackgroundOffColor, m_BackgroundOnColor, knobMovePercent);
			}
			else {
				m_Knob.color = m_KnobOnColor;
				m_Background.color = m_BackgroundOnColor;
			}

			var posX = ((m_StartX - m_EndX) * knobMovePercent) + m_EndX;
			m_KnobTransform.anchoredPosition = new Vector2(posX, m_KnobPosY);
		}

		private void FixedUpdate() {
			if (IsDisabled || !m_AnimateToggling) return;
			if (m_IsOn) {
				if (m_CurMoveX > .9899) return;
				m_CurMoveX = Mathf.Lerp(m_CurMoveX, 1, 15f * Time.deltaTime);
			}
			else {
				if (m_CurMoveX < .0001) return;
				m_CurMoveX = Mathf.Lerp(m_CurMoveX, 0, 15f * Time.deltaTime);
				
			}
			ChangeView(m_CurMoveX);
		}
		
		private void Setup() {
			m_KnobTransform = m_Knob.GetComponent<RectTransform>();
			var startPosition = m_KnobTransform.anchoredPosition;
			m_StartX = Mathf.Abs(startPosition.x);
			m_KnobPosY = startPosition.y;
			m_EndX = -m_StartX;
			if (m_ChangeColorOnSwitch) {
				if (m_IsOn) {
					m_Background.color = m_BackgroundOnColor;
					m_Knob.color = m_KnobOnColor;
				}
				else {
					m_Background.color = m_BackgroundOffColor;
					m_Knob.color = m_KnobOffColor;
				}
			}
			else {
				m_Background.color = m_BackgroundOnColor;
				m_Knob.color = m_KnobOnColor;
			}

			m_CurMoveX = m_IsOn ? 1 : 0;
			m_KnobTransform.anchoredPosition = new Vector2(m_IsOn ? m_StartX : m_EndX, m_KnobPosY);
			IsDisabled = IsDisabled;
		}


		protected override void OnValidate() {
			Setup();
		}

		public override void OnPointerClick(PointerEventData eventData) {
			IsOn = !IsOn;
		}
	}
}