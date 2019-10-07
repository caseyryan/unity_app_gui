using Components.Drawer;
using UnityEngine;
using UnityEngine.UI;

namespace Components {
	public class DrawerSettingsPanel : MonoBehaviour {

		[SerializeField] private Slider m_OpeningDelta;
		[SerializeField] private Slider m_ClosingDelta;
		[SerializeField] private Slider m_AnimationSpeed;
		[SerializeField] private Slider m_TimeBetweenChecks;
		[SerializeField] private Slider m_MinDistToOpen;
		[SerializeField] private Text m_OpeningDeltaText;
		[SerializeField] private Text m_ClosingDeltaText;
		[SerializeField] private Text m_AnimationSpeedText;
		[SerializeField] private Text m_TimeBetweenChecksText;
		[SerializeField] private Text m_MinDistToOpenText;

		private void Start() {
			
			m_OpeningDelta.value = NavigationDrawer.m_OpeningDelta;
			m_ClosingDelta.value = NavigationDrawer.m_ClosingingDelta;
			m_AnimationSpeed.value = NavigationDrawer.m_AnimationSpeed;
			m_TimeBetweenChecks.value = NavigationDrawer.m_TimeBetweenChecks;
			m_MinDistToOpen.value = NavigationDrawer.m_MinDistForQuickSwipeOpen;
			
			m_OpeningDeltaText.text = $"Дельта открытия {m_OpeningDelta.value:F2}";
			m_ClosingDeltaText.text = $"Дельта закрытия {m_ClosingDelta.value:F2}";
			m_AnimationSpeedText.text = $"Скорость анимации {m_AnimationSpeed.value:F2}";
			m_TimeBetweenChecksText.text = $"Время между проверками {m_TimeBetweenChecks.value:F3}";
			m_MinDistToOpenText.text = $"Мин растояние открытия свайпом {m_MinDistToOpen.value:F2}";
			
			m_OpeningDelta.onValueChanged.AddListener(value => {
				NavigationDrawer.m_OpeningDelta = value;
				m_OpeningDeltaText.text = $"Дельта открытия {value:F2}";
			});
			m_ClosingDelta.onValueChanged.AddListener(value => {
				NavigationDrawer.m_ClosingingDelta = value;
				m_ClosingDeltaText.text = $"Дельта закрытия {value:F2}";
			});
			m_AnimationSpeed.onValueChanged.AddListener(value => {
				NavigationDrawer.m_AnimationSpeed = value;
				m_AnimationSpeedText.text = $"Скорость анимации {value:F2}";
			});
			m_TimeBetweenChecks.onValueChanged.AddListener(value => {
				NavigationDrawer.m_TimeBetweenChecks = value;
				m_TimeBetweenChecksText.text = $"Время между проверками {value:F3}";
			});
			m_MinDistToOpen.onValueChanged.AddListener(value => {
				NavigationDrawer.m_MinDistForQuickSwipeOpen = value;
				m_MinDistToOpenText.text = $"Мин растояние открытия свайпом {value:F2}";
			});
			
		}
	}
}