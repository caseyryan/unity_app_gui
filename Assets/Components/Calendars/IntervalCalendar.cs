
using System;
using System.Collections.Generic;
using Components.Localization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Components.Calendars {
	
	[ExecuteInEditMode]
	public class IntervalCalendar : MonoBehaviour {

		[SerializeField, Tooltip("Use sunday as the first day of the week")] 
		private bool m_UseSundayAsWeekStart = false;
		[SerializeField, Tooltip("The color of sunday text")] 
		private Color m_SundayColor;
		[SerializeField, Tooltip("The color the text for any normal day of month")] 
		private Color m_NormalDayColor;
		[SerializeField, Tooltip("Current day color")] 
		private Color m_CurrentDayColor;
		[SerializeField, Tooltip("The color of mupliple selection marker")] 
		private Color m_MultipleSelectorColor;
		[SerializeField] private Text m_InstructionText;
		[SerializeField] private BigMonthView m_BigMonthView;
		[SerializeField] private bool m_BoldDaysFont = false;
		[SerializeField] private bool m_BoldWeekFont = false;
		[SerializeField] private Color m_BackgroundColor;
		[SerializeField, Tooltip("Bottom instruction. You may also use {{phraseId}} pattern for localization")] 
		private string m_BottomPromptText;
		
		[SerializeField, Tooltip("You may set the names directly, or use a {{dayId}} pattern," +
		    " in this case the names will be picked up from a localization dictionary")] 
		private List<string> m_DayNames = new List<string>() {
			"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"
		};
		[SerializeField, Tooltip("You may set the names directly, or use a {{monthId}} pattern," +
		                         " in this case the names will be picked up from a localization dictionary")] 
		private List<string> m_MonthNames = new List<string>() {
			"January", "February", "March", "April", "May", "June", 
			"July", "August", "September", "October", "November", "December"
		};
		[SerializeField, Tooltip("Whether or not the same colors will be automatically used for day numbers and names")] 
		private bool m_UseSameColorsForDaysAndNames = true;

		private DateTime m_MinDate;
		private DateTime m_MaxDate;
		
		
		private void Start() {
			OnValidate();
		}


		protected void OnValidate() {
			var image = gameObject.GetComponent<Image>();
			if (image != null) {
				image.color = m_BackgroundColor;
			}
			m_BigMonthView.AllowMultipleSelection = true;
			m_BigMonthView.UseSundaysAsWeekStart = m_UseSundayAsWeekStart;
			m_BigMonthView.DayNames = m_DayNames;
			m_BigMonthView.MonthNames = m_MonthNames;
			m_BigMonthView.SetTextStyles(m_NormalDayColor, 
				m_SundayColor, m_CurrentDayColor, m_MultipleSelectorColor, m_BoldDaysFont, 
				m_BoldWeekFont, m_UseSameColorsForDaysAndNames);
			m_BigMonthView.SetDate(DateTime.Now);
			m_InstructionText.text = m_BottomPromptText.Localize();
			//SetSelection(new DateTime(2019, 2, 26), new DateTime(2019, 3, 31));

		}
		/// <summary>
		/// Sets multiple selection of days creating a time span
		/// and highlights the selected dates with a marker
		/// </summary>
		/// <param name="minDate"></param>
		/// <param name="maxDate"></param>
		public void SetSelection(DateTime minDate, DateTime maxDate) {
			if (maxDate < minDate) {
				throw new ArgumentException("Max date can't be less than the min one");
			}
			m_MinDate = minDate;
			m_MaxDate = maxDate;
			m_BigMonthView.SetSelection(minDate, maxDate);
		}
		
	}
}