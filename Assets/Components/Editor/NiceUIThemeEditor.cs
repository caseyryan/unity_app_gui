namespace Components.Editor {
	using UnityEngine;
	using UnityEditor;

	public class NiceUiThemeEditor : EditorWindow {

		[SerializeField] private bool m_UseDarkTheme;
		[SerializeField] private bool m_EnableAnimatableControlsColorsGroup;
		[SerializeField] private bool m_EnableBackgroundColorsGroup;
		[SerializeField] private Color m_StartColor = new Color(0.0f, 0.380f, 0.356f);
		[SerializeField] private Color m_EndColor = new Color(0.0f, 0.8313f, 0.7803f);
		[SerializeField] private Color m_DisabledDarkShadeColor = new Color(0.17f, 0.17f, 0.17f);
		[SerializeField] private Color m_DisabledLightShadeColor = new Color(0.302f, 0.302f, 0.302f);
		[SerializeField] private Color m_LightControlBackgroundColor = new Color(0.9245f, 0.9245f, 0.9245f);
		[SerializeField] private Color m_DarkControlBackgroundColor = new Color(0.169f, 0.169f, 0.169f);
		[SerializeField] private Color m_DarkPageBackgroundColor = new Color(0.10f, 0.10f, 0.10f);
		[SerializeField] private Color m_LightPageBackgroundColor = new Color(0.90f, 0.90f, 0.90f);
	
		
		[MenuItem("NiceUI Kit/Theme Editor")]
		static void Init() {
			var window = (NiceUiThemeEditor)GetWindow(typeof(NiceUiThemeEditor));
			window.Show();
		}

		private void OnEnable() {
			UnityEngine.Debug.Log(ComponentUtils.GetProjectName());
			var data = EditorPrefs.GetString(ComponentUtils.GetProjectName(), JsonUtility.ToJson(this, false));
			JsonUtility.FromJsonOverwrite(data, this);
		}

		private void OnDisable() {
			var data = JsonUtility.ToJson(this, false);
			EditorPrefs.SetString(ComponentUtils.GetProjectName(), data);
		}

		private void OnGUI() {
			GUILayout.Label("Base theme settings", EditorStyles.boldLabel);
			GUILayout.Label("Whether or not to use dark background colors");
			m_UseDarkTheme = GUILayout.Toggle(m_UseDarkTheme, "Use dark theme");
			m_EnableAnimatableControlsColorsGroup = EditorGUILayout.BeginToggleGroup("Colors for animatable controls", m_EnableAnimatableControlsColorsGroup);
			m_StartColor = EditorGUILayout.ColorField("Icon and text start color", m_StartColor);
			m_EndColor = EditorGUILayout.ColorField("Icon and text end", m_EndColor);
			m_DisabledDarkShadeColor = EditorGUILayout.ColorField("Dark color for disabled components", m_DisabledDarkShadeColor);
			m_DisabledLightShadeColor = EditorGUILayout.ColorField("Light color for disabled components", m_DisabledLightShadeColor);
			GUILayout.Space(20);
			EditorGUILayout.EndToggleGroup();
			m_EnableBackgroundColorsGroup = EditorGUILayout.BeginToggleGroup("Background colors", m_EnableBackgroundColorsGroup);
			m_LightControlBackgroundColor = EditorGUILayout.ColorField("Light control background", m_LightControlBackgroundColor);
			m_DarkControlBackgroundColor = EditorGUILayout.ColorField("Dark control background", m_DarkControlBackgroundColor);
			m_DarkPageBackgroundColor = EditorGUILayout.ColorField("Dark page background", m_DarkPageBackgroundColor);
			m_LightPageBackgroundColor = EditorGUILayout.ColorField("Light page background", m_LightPageBackgroundColor);
			EditorGUILayout.EndToggleGroup();
			
			
			GUILayout.Label("Apply theme to all components", EditorStyles.label);
			if (GUILayout.Button("Apply Theme")) {
				if (EditorUtility.DisplayDialog("Confirmation",
					"These settings will be applied to all fields that " +
					"have attributes from the 'ComponentAttributes.cs' file attached " +
					"and whose 'UseGlobalThemeColors' field is set to true" +
					"\nDo you want to continue?", "Yes", "No")) {
					ApplySettings();
				}
			}
		}

		private void ApplySettings() {
			Debug.Log("APPLYING");
		}
		
	}
}