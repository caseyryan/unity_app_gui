using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Components {
	// цепляется к штатному InputField и задает ему формат под номер телефона или 
	// другое число
	public class InputNumberFormatter : MonoBehaviour {
		
		[Tooltip("The format which will be used for your number representation")]
		[SerializeField] private string m_Format = "(###) ###-##-##";
		private InputField m_InputField;
		private StringBuilder m_FormatBuilder;
		
		private void Start() {
			m_InputField = GetComponent<InputField>();
			m_InputField.characterLimit = m_Format.Length;
			m_InputField.onValueChanged.AddListener(OnValueChanged);
			m_InputField.contentType = InputField.ContentType.Standard;
			m_InputField.ForceLabelUpdate();
			m_FormatBuilder = new StringBuilder(m_Format);
		}
		private void OnValueChanged(string text) {
			text = FormatNumberText(text);
			var curText = m_InputField.text;
			if (curText.Equals(text)) {
				return;
			}
			m_InputField.text = text;
			m_InputField.caretPosition = text.Length;
		}
		
		private string FormatNumberText(string text) {
			var digits = string.Join("", text.ToCharArray().Where(char.IsDigit));
			var stringBuilder = new StringBuilder();
			var curDigit = 0;
			for (var i = 0; i < m_FormatBuilder.Length; i++) {
				var formatChar = m_Format[i];
				if (!char.IsDigit(formatChar) && !formatChar.Equals('#')) {
					if (curDigit < digits.Length)
						stringBuilder.Append(formatChar);
				}
				else {
					if (curDigit > digits.Length - 1) {
						return stringBuilder.ToString();
					}
					stringBuilder.Append(digits[curDigit]);
					curDigit++;
				}
			}
			return stringBuilder.ToString();
		}
	}
}