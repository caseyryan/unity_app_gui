using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Components {
	public static class Extensions {
		
		private static readonly CultureInfo m_Culture = new CultureInfo("en-US");
		
		public static void SetAsPassword(this InputField inputField) {
			inputField.contentType = InputField.ContentType.Password;
			inputField.ForceLabelUpdate();
		}
		public static void SetAsStandard(this InputField inputField) {
			inputField.contentType = InputField.ContentType.Standard;
			inputField.ForceLabelUpdate();
		}
		public static bool IsPassword(this InputField inputField) {
			return inputField.contentType == InputField.ContentType.Password;
		}
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
			var result = gameObject.GetComponent<T>();
			if (result == default(T)) {
				result = gameObject.AddComponent<T>();
			}
			return result;
		}
		public static bool HasComponent<T>(this GameObject gameObject) where T : Component {
			return gameObject.GetComponent<T>() != null;
		}
		public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB) {
			T tmp = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = tmp;
			return list;
		}

		public static int SwapWith(this int input, ref int target) {
			var temp = input;
			input = target;
			target = temp;
			return input;
		}

		/// <summary>
		/// returns the ordinal number of the day of the week the month has started on.
		/// The week here starts from a monday and ends with a sunday
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="useSundayAsWeekStart"></param>
		/// <returns></returns>
		public static int GetDayOfWeekMonthStartedOn(this DateTime dateTime, bool useSundayAsWeekStart) {
			var date = new DateTime(dateTime.Year, dateTime.Month, 1);
			if (useSundayAsWeekStart) return (int)date.DayOfWeek;
			
			switch (date.DayOfWeek) {
				case DayOfWeek.Monday:
					return 0;
				case DayOfWeek.Tuesday:
					return 1;
				case DayOfWeek.Wednesday:
					return 2;
				case DayOfWeek.Thursday:
					return 3;
				case DayOfWeek.Friday:
					return 4;
				case DayOfWeek.Saturday:
					return 5;
				case DayOfWeek.Sunday:
					return 6;
			}

			return 0;
		}
		/// <summary>
		/// Gets a date with a month preceeding to this
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static DateTime GetPreviousMonth(this DateTime dateTime) {
			return dateTime.AddMonths(-1);
		}
		/// <summary>
		/// Gets a date with a month right next to this
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static DateTime GetNextMonth(this DateTime dateTime) {
			return dateTime.AddMonths(1);
		}

		public static int GetNumDaysInMonth(this DateTime dateTime) {
			return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
		}
		
		// to simplify chaining
		public static float Abs(this float num) {
			return Math.Abs(num);
		}
		public static int Abs(this int num) {
			return Math.Abs(num);
		}
		public static int ToInt(this float num) {
			return (int)num;
		}
		/// <summary>
		/// Converts long number to their shorter string representation
		/// For exameple 1000 -> 1K, 1000000 -> 1M, 1300 -> 1.3K and so on
		/// </summary>
		/// <param name="number">Number to format</param>
		/// <returns></returns>
		public static string ToShorterView(this double number) {
			string[] numberNames = {"K", "M", "B"};
			var i = 0;
			while (number >= TenPower((i + 1) * 3)) {
				++i;
			}
			var result = "";
			if (i == 0) {
				var temp = number.ToString(CultureInfo.InvariantCulture);
				result = number >= 10 ? temp : Math.Round(number).ToString(CultureInfo.InvariantCulture);
			}
			else
				result = (number / TenPower(i * 3)).ToString(CultureInfo.InvariantCulture);

			var postfix = "";
			while (i >= 3) {
				postfix = "B" + postfix;
				i -= 3;
			}

			if (i > 0)
				postfix = numberNames[i - 1] + postfix;

			if (result.Length > 4)
				result = result.Substring(0, 4);
			if (result.Contains("."))
				result = result.TrimEnd('0');
			return result.TrimEnd('.') + postfix;
		}

		public static string ToShorterView(this float number) {
			return ((double) number).ToShorterView();
		}

		private static float TenPower(int power){
			return (float) Math.Pow(10, power);
		}
		
		public static string[] SplitToArray(this string inputString, string delimiter = ",") {
			return inputString.Split(new [] {delimiter}, StringSplitOptions.RemoveEmptyEntries)
				.Select(val => val.Trim())
				.Where(val => !string.IsNullOrWhiteSpace(val)).ToArray();
		}
		public static string[] SplitToArray(this string inputString, char delimiter = ',') {
			return inputString.Split(delimiter)
				.Select(val => val.Trim())
				.Where(val => !string.IsNullOrWhiteSpace(val)).ToArray();
		}
		public static bool IsEmpty(this string str) {
			return string.IsNullOrWhiteSpace(str);
		}

		public static float ToFloat(this string str) {
			string trimmed = str.Trim();
			if (string.IsNullOrWhiteSpace(trimmed)) return 0;
			return Convert.ToSingle(trimmed, m_Culture);
		}
		public static int ToInt32(this string str) {
			var trimmed = str.Trim();
			if (string.IsNullOrWhiteSpace(trimmed)) return 0;
			return Convert.ToInt32(trimmed, m_Culture);
		}
		public static long ToInt64(this string str) {
			var trimmed = str.Trim();
			if (string.IsNullOrWhiteSpace(trimmed)) return 0;
			return Convert.ToInt64(trimmed, m_Culture);
		}
	}
}