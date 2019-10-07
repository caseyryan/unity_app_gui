using System;

namespace Components.Attributes {
	
	
	/**
	 * this file contains attributes which must be applied to all your stuff which
	 * you wanna be affected by global theme settings
	 * if you apply some of them and the UserThemeColors field of the component
	 * is true (or missing at all), then your component will 
	 * receive the according theme value which you set in NiceUI Theme editor window
	 * 
	 * EXAMPLE:
	 * [SmallSizeText] public Text buttonText;
	 * 
	 * If you add this attribute to a text, the editor will find your Text field
	 * and apply a setting the corresponds to a small size text in your theme editor
	 * 
	 */


	// TEXT ATTRIBUTES use these on all Text and InputField
	// object fields whose font you wanna be affected by your global theme settings 
	
	[AttributeUsage(AttributeTargets.Field)]
	public class SmallSizeText : Attribute { }
	
	[AttributeUsage(AttributeTargets.Field)]
	public class MiddleSizeText : Attribute { }
	
	[AttributeUsage(AttributeTargets.Field)]
	public class BigSizeText : Attribute { }
	
	[AttributeUsage(AttributeTargets.Field)]
	public class LargeSizeText : Attribute { }
	
	// COLOR ATTRIBUTES use this on all Image and RectTransform fields (that contain Image components) 
	// that you wanna be affected by your global theme settings
	[AttributeUsage(AttributeTargets.Field)]
	public class StartColor : Attribute { }

	[AttributeUsage(AttributeTargets.Field)]
	public class EndColor : Attribute { }
	
	[AttributeUsage(AttributeTargets.Field)]
	public class DisabledDarkShade : Attribute { }
	
	[AttributeUsage(AttributeTargets.Field)]
	public class DisabledLightShade : Attribute { }
}