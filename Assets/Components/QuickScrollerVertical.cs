using System;
using Components.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Components {
	public class QuickScrollerVertical : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, 
		IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IScrollable {

		
		[SerializeField] private RectTransform m_ScrollContainer;
		[SerializeField] private RectTransform m_Viewport;
		[SerializeField, Tooltip("Indicates whether or not elastic stretch should be allowed from atop")] 
		private bool m_UserTopElastic = true;
		[SerializeField, Tooltip("Indicates whether or not elastic stretch should be allowed from abottom")] 
		private bool m_UseBottomElastic = true;
		[SerializeField, Tooltip("Deceleration rate after slow swipe")] 
		private float m_DecelerationRateNormal = 3.55f;
		[SerializeField, Tooltip("Deceleration rate after quick swipe")] 
		private float m_DecelerationRateQuick = 2.10f;
		[SerializeField, Tooltip("Used to determine quick and slow swipes")] 
		private float m_DelayBetweenPositionChecks = .062f;
		[SerializeField, Tooltip("Max scroll speed after any type of swipe")] 
		private float m_MaxSpeedOfQuickScroll = 446f;
		private float m_ViewportHeight = 0;
		private float m_ContainerHeight = 0;
		private bool m_IsDragging = false;
		private float m_SpeedY = 0;
		private float m_StartTouchTime = 0;
		private Vector2 m_StartTouchPosition = Vector2.zero;
		private bool m_IsEnabled = true;
		private Vector2 m_InitialPosition;
		private bool m_NormalDeceleration = true;
		private float m_TweenBackSpeed = 8.1f;
		private bool m_IsPointerDown = false;
		private readonly float m_MaxElasticOverflow = 800f;
		private float m_PointerDownStart = 0f;
		// to dispatch tap only after it stopped
		private bool m_JustStopped = false;
		// going back from elastic movement
		private float m_TweenEndValue = 0;
		private bool m_IsTweening = false;
		
		private float m_ReleaseTime = -1;
		// automatically releases elastic stretching after this time
		private readonly float m_ReleaseWaitTime = 0.8f;

		/// <summary>
		/// the distance of stretch movement overflow from top or bottom
		/// may be used to display loading indicator or something like that
		///
		/// usage example:
		/// scroller.OnOverflow += (scroller, distance, position) => {
		///    if (position == Position.Top) {
		///        print($"overflow from atop by {distance}"); 
		///    }
		/// }
		/// 
		/// </summary>
		public event UnityAction<QuickScrollerVertical, float, Position> OnOverflow;
		/// <summary>
		/// dispatched when the container reaches its ultimate position
		/// </summary>
		public event UnityAction<QuickScrollerVertical, Position> OnUltimatePositionReached;

		private void Start() {
			m_ViewportHeight = m_Viewport.rect.height;
			m_InitialPosition = m_ScrollContainer.anchoredPosition;
		}

		public bool IsEnabled {
			get { return m_IsEnabled; }
			set {
				m_IsEnabled = value;
				if (!value) {
					m_IsTweening = false;
					m_IsDragging = false;
					m_SpeedY = 0;
				}
			}
		}

		public bool IsDragging {
			get { return m_IsDragging; }
		}

		public bool IsMoving() {
			return m_SpeedY > 1;
		}

		public void OnDrag(PointerEventData eventData) {
			if (!m_IsEnabled) return;
			var moveX = eventData.delta.x.Abs();
			var moveY = eventData.delta.y.Abs();
			m_SpeedY = eventData.delta.y;
			if (moveX > moveY) {
				m_IsDragging = false;
				m_SpeedY = 0;
			}
			MoveBy(m_SpeedY);
		}
		/// <summary>
		/// Scrolls container to the very top
		/// </summary>
		public void ScrollToTop() {
			OnPointerUp(null);
			m_SpeedY = 0;
			m_TweenEndValue = m_InitialPosition.y;
			m_IsTweening = true;
			MoveBy(0);
		}
		

		private void MoveBy(float delta) {
			if (!m_IsEnabled) return;
			var pos = m_ScrollContainer.anchoredPosition;
			var maxY = m_ContainerHeight - m_ViewportHeight;
			float nextY = 0.0f;
			if (m_IsDragging) {
				if (pos.y < m_InitialPosition.y) {
					// elastic stretch from atop
					var leftToGo = pos.y + m_MaxElasticOverflow;
					var overflowPercent = Mathf.Clamp01((leftToGo / m_MaxElasticOverflow) * .55f);
					delta *= overflowPercent;
					
				} else if (pos.y > maxY) {
					var leftToGo = (maxY + m_MaxElasticOverflow) - pos.y;
					var overflowPercent = Mathf.Clamp01((leftToGo / m_MaxElasticOverflow) * .55f);
					delta *= overflowPercent;
				}
				nextY = pos.y + delta;
				if (!m_UserTopElastic) {
					if (nextY < m_InitialPosition.y) {
						nextY = m_InitialPosition.y;
					}
				}

				if (!m_UseBottomElastic) {
					if (nextY > maxY) {
						nextY = maxY;
					}
				}
				pos.y = nextY;
			}
			else {
				if (pos.y < m_InitialPosition.y) {
					// top
					m_SpeedY = 0;
					m_TweenEndValue = m_InitialPosition.y;
					m_IsTweening = true;
				} else if (pos.y > maxY) {
					// bottom
					m_SpeedY = 0;
					m_TweenEndValue = maxY;
					m_IsTweening = true;
				}
				nextY = pos.y + delta;
				if (!m_UserTopElastic) {
					if (nextY < m_InitialPosition.y) {
						nextY = m_InitialPosition.y;
					}
				}

				if (!m_UseBottomElastic) {
					if (nextY > maxY) {
						nextY = maxY;
					}
				}
				pos.y = nextY;
			}
			
			if (nextY < m_InitialPosition.y) {
				if (IsDragging) {
					if (!m_IsTweening) {
						OnOverflow?.Invoke(this, nextY - m_InitialPosition.y, Position.Top);
						if (Time.time > m_ReleaseTime) {
							m_ReleaseTime = Time.time + m_ReleaseWaitTime;
						}
					}
				}
				else {
					OnUltimatePositionReached?.Invoke(this, Position.Top);
				}
			} else if (nextY > maxY) {
				if (IsDragging) {
					if (!m_IsTweening) {
						OnOverflow?.Invoke(this, nextY - maxY, Position.Bottom);
						if (Time.time > m_ReleaseTime) {
							m_ReleaseTime = Time.time + m_ReleaseWaitTime;
						}
					}
				}
				else {
					OnUltimatePositionReached?.Invoke(this, Position.Bottom);
				}
			}
			m_ScrollContainer.anchoredPosition = pos;
		}

		public bool IsDoingElasticMovement() {
			return m_IsTweening;
		}

		public void Reset() {
			m_IsPointerDown = false;
			m_SpeedY = 0;
			m_IsDragging = false;
		}
		
		public void OnBeginDrag(PointerEventData eventData) {
			if (!m_IsEnabled) return;
			m_IsDragging = true;
			m_StartTouchTime = Time.time;
			m_StartTouchPosition = eventData.position;
		}

		public void OnEndDrag(PointerEventData eventData) {
			if (!m_IsEnabled) return;
			m_IsDragging = false;
			var t = Time.time;
			var timePassed = t - m_StartTouchTime;
			if (timePassed < m_DelayBetweenPositionChecks) {
				m_SpeedY = eventData.position.y - m_StartTouchPosition.y;
				m_SpeedY = Mathf.Abs(Mathf.Min(m_SpeedY, m_MaxSpeedOfQuickScroll)) * Mathf.Sign(m_SpeedY);
				m_NormalDeceleration = false;
			}
			else {
				m_NormalDeceleration = true;
			}

			MoveBy(0); // to tween back if there was an overflow
		}

		public void ResetPosition() {
			m_ScrollContainer.anchoredPosition = m_InitialPosition;
		}

		private void LateUpdate() {
			// container height recalculation
			if (Math.Abs(m_ScrollContainer.rect.height - m_ContainerHeight) > .01f) {
				m_ContainerHeight = Mathf.Max(m_ScrollContainer.rect.height, m_ViewportHeight + m_InitialPosition.y);
			}
			
			if (!m_IsEnabled) return;
			
			if (m_IsDragging && m_ReleaseTime > -1) {
				if (Time.time >= m_ReleaseTime) {
					m_ReleaseTime = -1;
					m_IsDragging = false;
					MoveBy(0);
				}
			}
			if (m_IsTweening) {
				var curPos = m_ScrollContainer.anchoredPosition;
				curPos.y = Mathf.Lerp(curPos.y, m_TweenEndValue, m_TweenBackSpeed * Time.deltaTime);
				m_ScrollContainer.anchoredPosition = curPos;
				if (Mathf.Abs(curPos.y - m_TweenEndValue) <= .005f) {
					m_IsTweening = false;
				}
			}
			
			
			if (IsDragging || Mathf.Abs(m_SpeedY) < .01f) return;
			if (m_IsPointerDown) {
				return;
			}
			var deceleration = m_NormalDeceleration 
				? m_DecelerationRateNormal 
				: m_DecelerationRateQuick;
			m_SpeedY = Mathf.Lerp(m_SpeedY, 0f, deceleration * Time.smoothDeltaTime);
			MoveBy(m_SpeedY);
		}
		public void OnPointerClick(PointerEventData eventData) {
			if (m_IsDragging) return;
			if (m_JustStopped) {
				m_JustStopped = false;
				return;
			}
			// если время между нажатием и отпусканием больше определенного, то не считать клик и не открывать акцию
			var clickDelay = Time.time - m_PointerDownStart;
			if (clickDelay <= .16f) {
				// todo dispatch tap here
			}
		}

		public void OnPointerDown(PointerEventData eventData) {
			m_ReleaseTime = -1;
			m_PointerDownStart = Time.time;
			m_IsTweening = false;
			m_IsPointerDown = true;
			if (Mathf.Abs(m_SpeedY) > 1) {
				m_JustStopped = true;
			}
			m_SpeedY = 0;
		}

		public void OnPointerUp(PointerEventData eventData) {
			m_ReleaseTime = -1;
			m_IsDragging = false;
			m_IsPointerDown = false;
		}
		
		public float ScrollProgress {
			get {
				var curPosY = Mathf.Abs(m_ScrollContainer.anchoredPosition.y);
				var maxMoveDistance = (m_InitialPosition.y + m_ContainerHeight) - m_ViewportHeight;
				return Mathf.Clamp01(curPosY / maxMoveDistance);
			}
		}
	}
}