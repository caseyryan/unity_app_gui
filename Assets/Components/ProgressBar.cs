using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Components {
	
	
	[ExecuteInEditMode]
	public class ProgressBar : ComponentBehaviour {
		
		[Tooltip("A suffix that will be appended to the progress value view")]
		[SerializeField] private string m_Suffix = "%";
		[Tooltip("Whether or not to use suffix")]
		[SerializeField] private bool m_UseSuffix = true;
		[SerializeField] private Image m_ProgressImage;
		[SerializeField] private float m_MinValue = 0.0f;
		[SerializeField] private float m_MaxValue = 100.0f;
		[SerializeField] private float m_Value = 0.0f;
		[SerializeField] private int m_DecimalPoints = 0;
		[SerializeField] private Text m_ProgressText;
		[SerializeField] private Color m_StartColor;
		[SerializeField] private Color m_EndColor;
		[Tooltip("If true and DecimalPoints value is > 0, the progress text will use comma as mantissa separator else it will use dot")]
		[SerializeField] private bool m_UseCommaAsTextSeparator = false;
		[Tooltip("Converts long numbers to their short equivalents like 1500 => 1.5K")]
		[SerializeField] private bool m_ShortenLongValues = false;
		[Tooltip("Will lerp from one value to another on change if true")]
		[SerializeField] private bool m_AnimateValueChange = false;
		private Color m_CurrentColor;
		private RectTransform m_RectTransform;
		private RectTransform m_ProgressTransform;
		// used for lerping only
		private float m_CurFillAmount;
		private string m_CurrentSuffix;
		public event UnityAction<float> ProgressUpdate; 


		protected virtual void Start() {
			if (m_ChangeColor) {
				m_CurrentColor = m_StartColor;
				SetColors();
			}
			OnValidate();
		}
		protected override void SetColors() {
			if (!m_ChangeColor) return;
			if (m_ProgressText != null) {
				m_ProgressText.color = m_CurrentColor;
			}
			ProgressImage.color = m_CurrentColor;

		}

		protected override void OnValidate() {
			Value = Value;
			UseSuffix = m_UseSuffix;
			
		}
		
		private void FixedUpdate() {
			if (!m_AnimateValueChange) return;
			if (Math.Abs(m_CurFillAmount - Value) < .001f) return;
			m_CurFillAmount = Mathf.Lerp(m_CurFillAmount, Value, 15f * Time.deltaTime);
			ChangeView(m_CurFillAmount);
		}

		public float Value {
			set {
				value = Mathf.Clamp(value, m_MinValue, m_MaxValue);
				if (!m_AnimateValueChange) {
					m_CurFillAmount = value;
					ChangeView(value);
				}
#if UNITY_EDITOR				
				else {
					if (!Application.isPlaying) {
						m_CurFillAmount = value;
						ChangeView(value);
					}
				}
#endif
				m_Value = value;
				ProgressUpdate?.Invoke(value);
			}
			get { return m_Value; }
		}

		public bool UseSuffix {
			get { return m_UseSuffix; }
			set {
				m_UseSuffix = value;
				m_CurrentSuffix = m_UseSuffix ? m_Suffix : string.Empty;
			}
		}

		protected RectTransform RectTransform {
			get {
				if (m_RectTransform == null) {
					m_RectTransform = GetComponent<RectTransform>();
				}
				return m_RectTransform;
			}
		}

		protected internal Image ProgressImage {
			get { return m_ProgressImage; }
		}
		protected RectTransform ProgressTransform {
			get {
				if (m_ProgressTransform == null) {
					m_ProgressTransform = m_ProgressImage.GetComponent<RectTransform>();
				}
				return m_ProgressTransform;
			}
		}

		/// <summary>
		/// Fills the progress image
		/// </summary>
		/// <param name="amount"></param>
		protected virtual void SetFillAmount(float amount) {
			ProgressImage.fillAmount = amount;
		}

		private void ChangeView(float value) {
			var fillAmount = value / m_MaxValue;
			
			SetFillAmount(fillAmount);
			if (m_ChangeColor) {
				m_CurrentColor = m_ChangeColor 
					? ComponentUtils.MixColorsByValue(m_StartColor, m_EndColor, fillAmount) 
					: m_EndColor;
				SetColors();
			}
			

			if (m_ProgressText == null) return;
			
			if (m_ShortenLongValues) {
				m_ProgressText.text = $"{Mathf.Round(value).ToShorterView()}{m_CurrentSuffix}";
			}
			else {
				var decimalFormat = $"F{m_DecimalPoints}";
				var culture = m_UseCommaAsTextSeparator ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
				m_ProgressText.text = $"{value.ToString(decimalFormat, culture)}{m_CurrentSuffix}";
			}
		}
		
		private void Update() {
			Value = m_Value;
		}
	}
	
}