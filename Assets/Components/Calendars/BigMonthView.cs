using System;
using System.Collections.Generic;
using System.Linq;
using Components.Clickable;
using Components.Texts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Components.Calendars {
	
	public class BigMonthView : InteractiveObject, IDragHandler, IEndDragHandler, IBeginDragHandler {
		
		[SerializeField] private RectTransform[] m_DayLines;
		[SerializeField] private RectTransform m_WeekLine;
		[SerializeField] private Image m_MultipleSelectorSampler;
		[SerializeField] private AnimatableButton m_MonthYearButton;
		[SerializeField] private AnimatableButton m_LeftArrowButton;
		[SerializeField] private AnimatableButton m_RightArrowButton;
		[SerializeField, Tooltip("An image that will be shown under current date")] 
		private Image m_CurDaySelector;

		private bool m_UseSundaysAsWeekStart = true;
		private List<CalendarDateCell> m_AllDaysTexts = new List<CalendarDateCell>();
		private List<Text> m_Week = new List<Text>();
		private DateTime m_Date;
		[SerializeField, Tooltip("Alpha for days that are not within the current month")] 
		private float m_OutOfRangeDayAlpha = .15f;
		[Tooltip("Allows to select a time span from one date to another")]
		private bool m_AllowMultipleSelection = false;
		// used to position date selection markers
		private readonly Dictionary<CalendarDateCell, Vector2> m_GridPositions = new Dictionary<CalendarDateCell, Vector2>();
		private bool m_IsSelecting = false;
		// the number of seconds after which the seelction will be enabled
		private float m_StartSelectingAfter = 0.55f;
		private List<string> m_MonthNames;
		private List<string> m_DayNames;
		[SerializeField] private Text m_MonthYearTitle;
		[SerializeField] private Text m_MiniMonthText;
		private readonly List<Image> m_SelectionMarkers = new List<Image>();
		// to keep track of the multiple selection
		private DateTime m_FirstDateOfSelection = DateTime.Today;
		private DateTime m_LastDateOfSelection = DateTime.Today;
		// used to detect selection position
		private Vector2 m_ClickOffset = Vector2.zero;
		private Vector2 m_StartTouchPosition = Vector2.zero;
		private CalendarDateCell m_FirstSelectedCell;
		private CalendarDateCell m_LastSelectedCell;
		private int m_FontSize = 36;
		private bool m_IsYearMode = false;
		

		private void Start() {
			if (m_SelectionMarkers.Count < 1) {
				for (var i = 0; i < 6; i++) {
					var marker = Instantiate(m_MultipleSelectorSampler);
					marker.transform.SetParent(m_MultipleSelectorSampler.transform.parent, false);
					m_SelectionMarkers.Add(marker);
				}
			}

			m_FontSize = m_MonthYearTitle.fontSize;
			m_MonthYearTitle.resizeTextForBestFit = false;
			
			m_MiniMonthText.gameObject.SetActive(false);
			m_LeftArrowButton.Click += OnButtonClick;
			m_RightArrowButton.Click += OnButtonClick;
			m_MonthYearButton.Click += OnButtonClick;
		}
		

		private void OnButtonClick(InteractiveObject sender, PointerEventData e) {
			var button = (AnimatableButton) sender;
			if (button == m_LeftArrowButton) {
				m_Date = IsIsYearMode
					? m_Date.AddYears(-1)
					: m_Date.AddMonths(-1);
				SetDate(m_Date);
			} 
			else if (button == m_RightArrowButton) {
				m_Date = IsIsYearMode 
					? m_Date.AddYears(1)
					: m_Date.AddMonths(1);
				SetDate(m_Date);
			} 
			else if (button == m_MonthYearButton) {
				IsIsYearMode = !IsIsYearMode;
			}
		}

		// also changes the height of button text and enables / disables mini month title
		private bool IsIsYearMode {
			set {
				m_IsYearMode = value;
				var size = m_MonthYearTitle.rectTransform.sizeDelta;
				var buttonHeight = m_MonthYearButton.GetComponent<RectTransform>().rect.height;
				if (value) {
					m_MonthYearTitle.text = ($"{m_Date.Year}");
					size.y = buttonHeight - m_MiniMonthText.rectTransform.rect.height;
					m_MonthYearTitle.rectTransform.sizeDelta = size;
					m_MonthYearTitle.resizeTextForBestFit = true;
					m_MonthYearTitle.resizeTextMaxSize = 60;
					m_MiniMonthText.text = $"{GetMonthName()}";
				}
				else {
					size.y = buttonHeight;
					m_MonthYearTitle.resizeTextForBestFit = false;
					m_MonthYearTitle.fontSize = m_FontSize;
					m_MonthYearTitle.rectTransform.sizeDelta = size;
					m_MonthYearTitle.text = ($"{GetMonthName()} {m_Date.Year}");
				}
				m_MiniMonthText.gameObject.SetActive(value);
			}
			get { return m_IsYearMode; }
		}
		

		private void FillDayTexts() {
			if (!m_AllDaysTexts.Any()) {
				foreach (var rectTransform in m_DayLines) {
					var allTexts = rectTransform.GetComponentsInChildren<CalendarDateCell>(true);
					m_AllDaysTexts.AddRange(allTexts);
				}
			}

			if (!m_Week.Any()) {
				m_Week.AddRange(m_WeekLine.GetComponentsInChildren<Text>(true));
			}

			m_MultipleSelectorSampler.gameObject.SetActive(false);
			PrecalculateCellPositions();

		}
		// precalculates positions for each cell, to simplify date selection
		private void PrecalculateCellPositions() {
			m_GridPositions.Clear();
			var startOffset = m_DayLines[0].anchoredPosition;
			m_ClickOffset = startOffset;
			m_ClickOffset.x += m_MultipleSelectorSampler.rectTransform.rect.width * .5f;
			m_ClickOffset.y -= m_MultipleSelectorSampler.rectTransform.rect.height * .5f;
			var cellWidth = m_DayLines[0].rect.width / 7;
			var lineHeight = m_DayLines[0].rect.height;
			// realigning all text cells. Necessary because the built-in horizontal layout group is buggy
			// and might position texts in wrong places depending on their contents
			for (var i = 0; i < m_AllDaysTexts.Count; i++) {
				var calendarDateText = m_AllDaysTexts[i];
				var colIndex = (i % 7);
				var rowIndex = i / 7;
				var line = m_DayLines[rowIndex];
				// to simplify marker position detection
				calendarDateText.GridRow = rowIndex;
				calendarDateText.GridColumn = colIndex;
				calendarDateText.CellWidth = cellWidth;
				calendarDateText.CellHeight = lineHeight;
				var pivot = calendarDateText.rectTransform.pivot;
				var posX = colIndex * cellWidth;
				var sizeDelta = calendarDateText.rectTransform.sizeDelta;
				sizeDelta.x = cellWidth;
				calendarDateText.rectTransform.sizeDelta = sizeDelta;
				var pos = calendarDateText.rectTransform.anchoredPosition;
				pos.x = posX + (cellWidth * pivot.x);
				pos.y = -pivot.y * lineHeight;
				calendarDateText.rectTransform.anchoredPosition = pos;

				var gridY = line.anchoredPosition.y - (lineHeight / 2);
				m_GridPositions.Add(calendarDateText, 
					new Vector2(
						pos.x + startOffset.x, 
						gridY
					)
				);
			}
			// realigning week names
			for (var i = 0; i < m_Week.Count; i++) {
				var weekTextField = m_Week[i];
				var pivot = weekTextField.rectTransform.pivot;
				var sizeDelta = weekTextField.rectTransform.sizeDelta;
				sizeDelta.x = cellWidth;
				weekTextField.rectTransform.sizeDelta = sizeDelta;
				var posX = (i % 7) * cellWidth;
				var pos = weekTextField.rectTransform.anchoredPosition;
				pos.x = posX + (cellWidth * pivot.x);
				weekTextField.rectTransform.anchoredPosition = pos;
			}
		}
		
		
		public void SetTextStyles(Color normalDayColor, Color sundayColor, 
			Color curDayColor, Color multipleSelectorColor,
			bool boldDaysFont = false, bool boldWeekFont = false, bool useSameStyleForDaysAndWeek = true) {
			FillDayTexts();

			SetSelectorColors(curDayColor, multipleSelectorColor);
			
			for (var i = 0; i < m_AllDaysTexts.Count; i++) {
				var isSunday = (i + (UseSundaysAsWeekStart ? 0 : 1)) % 7 == 0;
				var text = m_AllDaysTexts[i];
				text.color = isSunday ? sundayColor : normalDayColor;
				text.fontStyle = boldDaysFont ? FontStyle.Bold : FontStyle.Normal;
			}

			m_MonthYearTitle.color = normalDayColor;
			// setting color for mini month field which is displayer when the year selection mode is active
			var c = normalDayColor;
			c.a = m_OutOfRangeDayAlpha;
			m_MiniMonthText.color = c;

			if (useSameStyleForDaysAndWeek) {
				for (var i = 0; i < m_Week.Count; i++) {
					var isSunday = (i + (UseSundaysAsWeekStart ? 0 : 1)) % 7 == 0;
					var text = m_Week[i];
					text.color = isSunday ? sundayColor : normalDayColor;
					text.fontStyle = boldWeekFont ? FontStyle.Bold : FontStyle.Normal;
				}
			}

			IsIsYearMode = IsIsYearMode;
		}

		private void SetSelectorColors(Color curDayColor, Color multipleSelectorColor) {
			m_CurDaySelector.color = curDayColor;
			m_MultipleSelectorSampler.color = multipleSelectorColor;
		}
		
		// indicates the min and max dates that are visually available in this calendar grid
		// used for marker selection
		public DateTime MinVisibleDate {
			get {
				if (m_AllDaysTexts.All(c => c.monthType != MonthType.Previous))
					return new DateTime(m_Date.Year, m_Date.Month, 1);
				{
					var minDay = m_AllDaysTexts.Where(c => c.monthType == MonthType.Previous)
						.Min(calendarDateText => calendarDateText.Day);
					return new DateTime(m_Date.Year, m_Date.Month - 1, minDay);
				}
			}
		}

		public DateTime MaxVisibleDate {
			get {
				if (m_AllDaysTexts.All(c => c.monthType != MonthType.Next))
					return new DateTime(m_Date.Year, m_Date.Month, m_Date.GetNumDaysInMonth());
				{
					var minDay = m_AllDaysTexts.Where(c => c.monthType == MonthType.Next)
						.Max(calendarDateText => calendarDateText.Day);
					return new DateTime(m_Date.Year, m_Date.Month + 1, minDay);
				}
			}
		}

		private string GetMonthName() {
			return m_MonthNames[m_Date.Month - 1];
		}

		private bool IsThisMonthDisplaying() {
			var today = DateTime.Today;
			return m_Date.Month == today.Month && m_Date.Year == today.Year;
		}		
		
		/// <summary>
		/// Sets a date to display
		/// </summary>
		/// <param name="date"></param>
		public void SetDate(DateTime date) {
			m_Date = date;
			if (IsIsYearMode) {
				m_MonthYearTitle.text = ($"{m_Date.Year}");
			}
			else {
				m_MonthYearTitle.text = ($"{GetMonthName()} {m_Date.Year}");
			}
			
			var monthBeginWeekDay = date.GetDayOfWeekMonthStartedOn(UseSundaysAsWeekStart);
			var loopStart = monthBeginWeekDay;
			if (loopStart < 2) {
				// in order to leave a few day buffer at the beginnig and at the end of the current month
				loopStart += 7;

			}
			var loopEnd = NumDaysInSelectedMonth + loopStart;
			
			// filling current month days grid
			var day = 1;
			for (var i = loopStart; i < loopEnd; i++) {
				var calendarDateText = m_AllDaysTexts[i];
				calendarDateText.monthType = MonthType.Current;
				var color = calendarDateText.color;
				color.a = 1.0f;
				calendarDateText.color = color;
//				
				if (day == date.Day) {
					PositionCurrentDaySelector(calendarDateText);
				}
				calendarDateText.Day = day;
				day++;
			}
			
			// the days that are not within the current month
			// filling previous month dates
			
			var previousMonth = date.GetPreviousMonth();
			var daysInPrevMonth = previousMonth.GetNumDaysInMonth();
			day = (daysInPrevMonth - loopStart) + 1;
			for (var i = 0; i < loopStart; i++) {
				var calendarDateText = m_AllDaysTexts[i];
				var color = calendarDateText.color;
				calendarDateText.monthType = MonthType.Previous;
				// setting its alpha to a half value
				color.a = m_OutOfRangeDayAlpha;
				calendarDateText.color = color;
				calendarDateText.Day = day;
				day++;
			}
			
			// filling the rest of the griid with a next month's values
			if (loopEnd < m_AllDaysTexts.Count - 1) {
				day = 1;
				loopStart = loopEnd;
				loopEnd = m_AllDaysTexts.Count;
				
				for (var i = loopStart; i < loopEnd; i++) {
					var calendarDateText = m_AllDaysTexts[i];
					var color = calendarDateText.color;
					calendarDateText.monthType = MonthType.Next;
					// setting its alpha to a half value
					color.a = m_OutOfRangeDayAlpha;
					calendarDateText.color = color;
					calendarDateText.Day = day;
					day++;
				}
			}
		}

		private void PositionCurrentDaySelector(CalendarDateCell targetCellField) {
			var pos = m_GridPositions[targetCellField];
			m_CurDaySelector.rectTransform.anchoredPosition = pos;
			m_CurDaySelector.gameObject.SetActive(IsThisMonthDisplaying());
		}

		public override void OnPointerDoubleClick(PointerEventData eventData) {
			base.OnPointerDoubleClick(eventData);
			ClearSelectionMarkers();
		}

		public int NumDaysInSelectedMonth {
			get { return m_Date.GetNumDaysInMonth(); }
		}
		
		
		public bool UseSundaysAsWeekStart {
			get { return m_UseSundaysAsWeekStart; }
			set {
				FillDayTexts();
				if (value != m_UseSundaysAsWeekStart) {
					m_UseSundaysAsWeekStart = value;
					if (DayNames != null) {
						DayNames = DayNames;
					}
				}
			}
		}
		
		public void SetSelection(DateTime minDate, DateTime maxDate) {
			if (!m_SelectionMarkers.Any()) {
				//print("Markers will only be available on app launch!");
				return;
			}
			if (maxDate < minDate) {
				throw new ArgumentException("Max date can't be less than the min one");
			}
			var minVisibleDate = MinVisibleDate;
			var maxVisibleDate = MaxVisibleDate;
			if (minDate < minVisibleDate) minDate = minVisibleDate;
			else if (minDate > maxVisibleDate) minDate = maxVisibleDate;
			
			if (maxDate > maxVisibleDate) maxDate = maxVisibleDate;
			else if (maxDate < minVisibleDate) maxDate = minVisibleDate;

			m_FirstDateOfSelection = minDate;
			m_LastDateOfSelection = maxDate;
			
			var minDateType = GetMonthTypeForDate(minDate);
			var maxDateType = GetMonthTypeForDate(maxDate);
			var startCell = m_AllDaysTexts.FirstOrDefault(c => c.Day == minDate.Day && c.monthType == minDateType);
			var endCell = m_AllDaysTexts.FirstOrDefault(c => c.Day == maxDate.Day && c.monthType == maxDateType);
			
			DrawSelectionMarker(startCell, endCell);
		}

		private MonthType GetMonthTypeForDate(DateTime dateTime) {
			if (dateTime.Month == m_Date.Month) return MonthType.Current;
			if (dateTime.Month == m_Date.Month - 1) return MonthType.Previous;
			if (dateTime.Month == m_Date.Month + 1) return MonthType.Next;
			return MonthType.None;
		}

		private void ClearSelectionMarkers() {
			m_SelectionMarkers.ForEach(image => image.gameObject.SetActive(false));
		}
		private void SwapCells(ref CalendarDateCell first, ref CalendarDateCell second) {
			var temp = first;
			first = second;
			second = temp;
		}

		// draws the actual selection
		private void DrawSelectionMarker(CalendarDateCell startCell, CalendarDateCell endCell) {
			if (startCell == null || endCell == null) return;
			ClearSelectionMarkers();
			
			// to enable backward selection
			if (startCell.GridRow > endCell.GridRow) {
				SwapCells(ref startCell, ref endCell);
			} else if (startCell.GridRow == endCell.GridRow) {
				if (startCell.GridColumn > endCell.GridColumn) {
					SwapCells(ref startCell, ref endCell);
				}
			}
			m_FirstDateOfSelection = GetDateBySelectedCell(startCell);
			m_LastDateOfSelection = GetDateBySelectedCell(endCell);
			
			// we're gonna need a few markers for a multiline selection
			var numRowsUsed = Mathf.Abs(startCell.GridRow - endCell.GridRow) + 1;
			var loopStart = startCell.GridRow;
			var loopEnd = numRowsUsed + startCell.GridRow;
			
			for (var row = loopStart; row < loopEnd; row++) {
				var startMarker = m_SelectionMarkers[row];
				startMarker.gameObject.SetActive(false);
				GetCellSpanForRow(row, out var rowStartCell, out var rowEndCell,
					startCell, endCell, loopStart, loopEnd - 1);
				if (rowStartCell == null || rowEndCell == null) continue;
				var markerLength = GetMultipleSelectionMarkerLength(rowStartCell, rowEndCell);
				// there are only 6 markers one for each row
				var xOffset = startMarker.rectTransform.rect.height / 2;
				var startPosition = m_GridPositions[rowStartCell];
				startPosition.x -= xOffset;
				startMarker.rectTransform.anchoredPosition = startPosition;
				var size = startMarker.rectTransform.sizeDelta;
				size.x = markerLength;
				startMarker.rectTransform.sizeDelta = size;
				startMarker.gameObject.SetActive(true);
				startMarker.transform.SetAsFirstSibling();
			}
		}

		// сюда передается номер ряда и начальная и конечная ячейка.
		// А метод проверяет сколько ячеек между ними, входят в конкретный ряд
		// и возвращаяет начальную и конечную ячейку в переделах указанного ряда
		// нужно для многострочного выделения
		// finds first and last cells that are within the specified time span and lay within the current row
		private void GetCellSpanForRow(int curRow, out CalendarDateCell rowStartCell, out CalendarDateCell rowEndCell,
			CalendarDateCell startCell, CalendarDateCell endCell, int rowStart, int rowEnd) {

			rowStartCell = null;
			rowEndCell = null;

			var row = m_AllDaysTexts
				.Where(c => c.GridRow == curRow)
				.OrderBy(c => c.rectTransform.anchoredPosition.x).ToList();
			
			var bothInSameRow = rowStart == rowEnd;
			if (!bothInSameRow) {
				if (rowStart == curRow) {
					// первый точно должен быть в этом ряду
					rowStartCell = row.First(c => c.Day == startCell.Day);
					rowEndCell = row.Last();
				}
				else if (curRow != rowStart && curRow != rowEnd) {
					// этот ряд должен быть заполнен полностью
					rowStartCell = row.First();
					rowEndCell = row.Last();
				}
				else {
					// в этом ряду должен быть только последний
					rowStartCell = row.First();
					rowEndCell = row.First(c => c.Day == endCell.Day);
				}
			}
			else {
				// оба в одном ряду
				rowStartCell = row.FirstOrDefault(c => c.Day == startCell.Day);
				rowEndCell = row.LastOrDefault(c => c.Day == endCell.Day);
			}
		}
		
		// calculates distance between cells within a single row. It's used for multiple selection markers
		private float GetMultipleSelectionMarkerLength(CalendarDateCell startCell, CalendarDateCell endCell) {
			var offset = m_MultipleSelectorSampler.rectTransform.rect.height;
			return (m_GridPositions[endCell].x - m_GridPositions[startCell].x) + offset;
		}
		
		public override void OnPointerUp(PointerEventData eventData) {
			m_IsSelecting = false;
			m_FirstSelectedCell = null;
			m_LastSelectedCell = null;
			m_StartTouchPosition = Vector2.zero;
			CancelInvoke(nameof(EnableSelection));
			base.OnPointerUp(eventData);
		}

		public override void OnPointerDown(PointerEventData eventData) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				GetRectTransform(), eventData.position, eventData.enterEventCamera, out m_StartTouchPosition);
			Invoke(nameof(EnableSelection), m_StartSelectingAfter);
			base.OnPointerDown(eventData);
		}

		private void EnableSelection() {
			m_IsSelecting = true;
			ClearSelectionMarkers();
			if (m_StartTouchPosition != Vector2.zero) {
				m_FirstSelectedCell = FindCellNearestToTouchPosition(m_StartTouchPosition);
				if (m_FirstSelectedCell != null) {
					m_LastSelectedCell = m_FirstSelectedCell;
					DrawSelectionMarker(m_FirstSelectedCell, m_FirstSelectedCell);
				}
			}
		}

		private DateTime GetDateBySelectedCell(CalendarDateCell targetCell) {
			if (targetCell == null || targetCell.monthType == MonthType.None) {
				return DateTime.Today;
			}
			switch (targetCell.monthType) {
				case MonthType.Current:
					return new DateTime(m_Date.Year, m_Date.Month, targetCell.Day);
				case MonthType.Previous:
					return new DateTime(m_Date.Year, m_Date.Month - 1, targetCell.Day);
				case MonthType.Next:
					return new DateTime(m_Date.Year, m_Date.Month + 1, targetCell.Day);
				default:
					return DateTime.Today;
			}
		}
		

		public bool AllowMultipleSelection {
			get { return m_AllowMultipleSelection; }
			set { m_AllowMultipleSelection = value; }
		}

		public List<string> MonthNames {
			get { return m_MonthNames; }
			set { m_MonthNames = value; }
		}

		/// <summary>
		/// Sets the names for each week day. The array must be starting with
		/// monday and end with sunday. E.g. { Mon, Tue, Wed, Thu, Fri, Sat, Sun }
		/// or, for russian day names it should be { Пн, Вт, Ср, Чт, Пт, Сб, Вс }
		/// </summary>
		public List<string> DayNames {
			get { return m_DayNames; }
			set {
				m_DayNames = value;
				// to leave original list unchanged, just copy it
				var names = m_DayNames.ToList();
				if (UseSundaysAsWeekStart) {
					// the names array must always start with monday, so if you decide to use sunday as week start
					// it just adds sunday to the beginning of the list
					var sundayName = names[6];
					names.RemoveAt(6);
					names.Insert(0, sundayName);
				}
				for (var i = 0; i < names.Count; i++) {
					m_Week[i].text = names[i];
				}
			}
		}
		
		/// <summary>
		/// Returns minimum date of selection
		/// </summary>
		public DateTime FirstDateOfSelection {
			get { return m_FirstDateOfSelection; }
		}
		/// <summary>
		/// Returns maximum date of selection
		/// </summary>
		public DateTime LastDateOfSelection {
			get { return m_LastDateOfSelection; }
		}

		public void OnDrag(PointerEventData eventData) {
			if (m_IsSelecting) {
				if (AllowMultipleSelection) {
					if (m_FirstSelectedCell == null) return;
					// blocks drag events for parents while selecting, preventing scrolling etc.
					RectTransformUtility.ScreenPointToLocalPointInRectangle(
						GetRectTransform(), eventData.position, eventData.enterEventCamera, out var touchPos);
					m_LastSelectedCell = FindCellNearestToTouchPosition(touchPos);
				}
				else {
					// select single cell
					RectTransformUtility.ScreenPointToLocalPointInRectangle(
						GetRectTransform(), eventData.position, eventData.enterEventCamera, out var touchPos);
					m_LastSelectedCell = FindCellNearestToTouchPosition(touchPos);
					m_FirstSelectedCell = m_LastSelectedCell;
				}
				DrawSelectionMarker(m_FirstSelectedCell,
					m_LastSelectedCell != null ? m_LastSelectedCell : m_FirstSelectedCell);
			}
			else {
				if (eventData.delta.magnitude > 4.0f) {
					// allows not to enable selection while moving
					CancelInvoke(nameof(EnableSelection));
					Invoke(nameof(EnableSelection), m_StartSelectingAfter);
				}
				ForwardToParents<IDragHandler>(parent => parent.OnDrag(eventData));
			}
		}

		private CalendarDateCell FindCellNearestToTouchPosition(Vector2 touchPos) {
			var minDistance = 1000.0f;
			CalendarDateCell nearestCell = null;
			foreach (var keyValuePair in m_GridPositions) {
				var gridPos = keyValuePair.Value + m_ClickOffset;
				var dist = Vector2.Distance(gridPos, touchPos);
				if (dist < minDistance) {
					minDistance = dist;
					nearestCell = keyValuePair.Key;
				}
			}
			return nearestCell;
		}

		public void OnEndDrag(PointerEventData eventData) {
			if (!m_IsSelecting) {
				ForwardToParents<IEndDragHandler>(parent => parent.OnEndDrag(eventData));
			}
		}

		public void OnBeginDrag(PointerEventData eventData) {
			ForwardToParents<IBeginDragHandler>(parent => parent.OnBeginDrag(eventData));
		}
	}
}