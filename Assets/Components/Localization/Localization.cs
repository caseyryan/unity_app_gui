using UnityEngine.UI;

namespace Components.Localization {
	public static class Localization {

		
		
		
		/// <summary>
		/// Simply call it on a string a to localize.
		/// If the string is pattern like {{phraseId}} surrounded with double curly braces
		/// the internal text will be considered as a dictionary key and will be looked up in a
		/// localization dictionary. If the correcponding key is found it will be replaced with a translation
		/// </summary>
		/// <param name="stringToLocalize"></param>
		public static string Localize(this string stringToLocalize) {
			
			// todo сделать нормальную локализацию
			return stringToLocalize;
		}
		
		
	}
}