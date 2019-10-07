using UnityEngine.UI;

namespace Components.Texts {
	
	public enum MonthType {
		Previous,
		Current,
		Next,
		None
	}

	// this class is used internally by BigMonthView as a grid cell holder
	// you don't need to use it yourself
	public class CalendarDateCell : Text {
		private int m_Day;
		// to simplify marker position detection
		public int GridColumn { get; set; }
		public int GridRow { get; set; }
		public float CellWidth { get; set; }
		public float CellHeight { get; set; }

		public int Day {
			get { return m_Day; }
			set {
				m_Day = value;
				this.text = value.ToString();
			}
		}

		public MonthType monthType { get; set; } = MonthType.None;
	}
}