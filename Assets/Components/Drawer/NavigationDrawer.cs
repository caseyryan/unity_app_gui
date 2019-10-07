using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Components.Clickable;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Components.Drawer {
	
	
//	[ExecuteInEditMode]
	public class NavigationDrawer : InteractiveObject, IDragHandler, IBeginDragHandler, IEndDragHandler {

		// this is used to determin which of the drawers should update veil alpha
		private static NavigationDrawer m_CurrentlyActiveDrawer = null;
		
		[SerializeField] private RectTransform m_SidePanel;
		[SerializeField] private RectTransform m_Veil;
		[SerializeField] private InteractiveObject m_Grip;
		private RectTransform m_GripTransform;
//		[SerializeField] private RectTransform m_MovablePanel;
		[SerializeField] private float m_MaxAlpha = .7f;
		[SerializeField] private float m_FadingSpeed = .057f;
		[Tooltip("How much the drawer must be dragged out to make it go to the open position when finger is released")]
		public static float m_OpeningDelta = .45f;
		public static float m_ClosingingDelta = .55f;
		[Tooltip("The value that indicates when the drawer should be able to open/close by itself when a finger is released")]
		private float m_StartMoveDelta;
		[Tooltip("Touchable part which allows to drag the drawer out of the side position")]
		[SerializeField] private float m_GripWidth = 30f;
		[FormerlySerializedAs("m_DrawerPosition")] [SerializeField] private NavigationDrawerPosition m_NavigationDrawerPosition = NavigationDrawerPosition.Left;
		private float m_DrawerWidth;
		[SerializeField] private CanvasGroup m_VeilCanvasGroup;

		[Tooltip("If true, the drawer will always be open")] 
		[SerializeField] private bool m_IsAlwaysOpen = false;
		[SerializeField] private Text m_TempText;
		private float m_CurMoveSpeed = 0;
		// allows to close the drawer after this movement buffer is overfilled
		private float m_HorSlideBuffer = 0;
		private InteractiveObject m_DrawerComponent;
		private bool m_IsDragging = false;
		private float m_MaxGripWidth;
		private bool m_IsMovingHorizontally = true;
		private float m_TotalWidth;
		// while moving inner contents vertically it must be impossible to open or close the drawer
		private bool m_IsMovingVertically;
		// used to control open and close events
		private bool m_IsOpenSent = false;
		private bool m_IsCloseSent = false;
		// must be attached to a parent container
		private NavigationDrawerPanel m_NavigationDrawerPanel;
		// for lerping
		private float m_CurrentX;
		private float m_FinalX;
		private float m_OpenPercent = 0.0f;

		[Tooltip("A button or some other interactive object that will open and close the drawer")]
		[SerializeField] private InteractiveObject m_ControlButton;
		public static float m_AnimationSpeed = 21.0f;
		public static float m_TimeBetweenChecks = .50f;
		// если движение закончилось раньше этого времени, то это считается быстрым движением
		private float m_DragShouldStopBefore = 0;
		public static float m_MinDistForQuickSwipeOpen = 50.0f;
		private Vector2 m_DragStartPosition = Vector2.zero;
		private bool m_IsAnimating = false;


		public event UnityAction<NavigationDrawer> Close;
		public event UnityAction<NavigationDrawer> Open; 
		

		private void Awake() {
			m_GripTransform = m_Grip.gameObject.GetComponent<RectTransform>();
			m_Grip.PointerDown += OnGripPointerDown;
			m_Grip.PointerUp += OnGripPointerUp;
			Setup();
		}
		
		private void Setup() {
			if (m_ControlButton != null) {
				m_ControlButton.Click += OnControlClick;
			}
			
			m_DrawerWidth = m_SidePanel.sizeDelta.x;
			var rect = m_Veil.rect;
			m_TotalWidth = rect.width;
			var curSize = m_GripTransform.sizeDelta;
			m_GripTransform.sizeDelta = new Vector2(m_GripWidth, curSize.y);
			// when fully open the grip size is increased to simplify tapping on it
			m_MaxGripWidth = rect.width - m_SidePanel.rect.width;
			m_StartMoveDelta = m_OpeningDelta;
			SetupAnchors();
			m_Grip.gameObject.SetActive(!m_IsAlwaysOpen);
			if (!m_IsAlwaysOpen) {
				SetClosed();
			}
			else {
				SetOpen();
			}
			m_CurrentX = m_SidePanel.anchoredPosition.x;
			m_FinalX = m_CurrentX;
		}

		private void OnControlClick(InteractiveObject target, PointerEventData eventData) {
			if (target == m_ControlButton) {
				if (IsOpen) DoClose();
				else if (IsClosed) DoOpen();
			}
		}

		private void SetupAnchors() {
			var pivot = new Vector2(0, 1);
			var anchorMin = new Vector2(0, 0);
			var anchorMax = new Vector2(0, 1);
			var gripPivot = new Vector2(0, .5f);
			var gripPositionX = m_DrawerWidth * .5f;
			if (m_NavigationDrawerPosition == NavigationDrawerPosition.Right) {
				pivot = new Vector2(1, 1);
				anchorMin = new Vector2(1, 0);
				anchorMax = new Vector2(1, 1);
				gripPivot = new Vector2(1, .5f);
				gripPositionX = m_DrawerWidth * -.5f;
			}
			m_SidePanel.pivot = pivot;
			m_SidePanel.anchorMin = anchorMin;
			m_SidePanel.anchorMax = anchorMax;
			m_GripTransform.pivot = gripPivot;
			m_GripTransform.anchoredPosition = new Vector2(gripPositionX, 0);
		}

		private void ChangeGripWidth(bool toMax) {
			var curSize = m_GripTransform.sizeDelta;
			m_GripTransform.sizeDelta = toMax ? new Vector2(m_MaxGripWidth, curSize.y) : new Vector2(m_GripWidth, curSize.y);
		}

		public override void OnPointerDown(PointerEventData eventData) {
			base.OnPointerDown(eventData);
			m_IsAnimating = false;
			m_HorSlideBuffer = 0;
			m_IsDragging = false;
			m_CurMoveSpeed = 0;
		}

		private void OnGripPointerDown(InteractiveObject target, PointerEventData eventData) {
			m_HorSlideBuffer = 0;
//			m_DragStartPosition = eventData.position;
			m_IsDragging = false;
			m_CurMoveSpeed = 0;
			if (target == m_Grip) {
				transform.SetAsLastSibling();
				m_CurrentlyActiveDrawer = this;
				var posX = Mathf.Round(m_SidePanel.anchoredPosition.x);
				// peeping from behind a side on touch
				if (m_NavigationDrawerPosition == NavigationDrawerPosition.Left) {
					if (posX <= -m_DrawerWidth) {
						m_FinalX = -(m_DrawerWidth - m_GripWidth);
						LerpPanel(m_AnimationSpeed, CalculateOpenPercentAndVeilAlpha);
						//m_SidePanel.DOAnchorPosX(-(m_DrawerWidth - m_GripWidth), .15f).SetEase(m_Easing).OnUpdate(UpdateVeilAlpha);
					}
				}
				else {
					if (posX >= m_DrawerWidth) {
						m_FinalX = m_DrawerWidth - m_GripWidth;
						LerpPanel(m_AnimationSpeed, CalculateOpenPercentAndVeilAlpha);
						//m_SidePanel.DOAnchorPosX(m_DrawerWidth - m_GripWidth, .15f).SetEase(m_Easing).OnUpdate(UpdateVeilAlpha);
					}
				}
			}
			
		}
		private void OnGripPointerUp(InteractiveObject target, PointerEventData eventData) {
			if (target == m_Grip) {
				if (!m_IsDragging) {
					DoClose();
				}
			}
		}
		
		public void OnBeginDrag(PointerEventData eventData) {
			m_DragStartPosition = eventData.position;
			m_DragShouldStopBefore = Time.time + m_TimeBetweenChecks;
			m_HorSlideBuffer = 0;
			m_IsDragging = true;
			m_CurMoveSpeed = 0;
		}

		private void FixedUpdate() {
			if (m_IsMovingHorizontally) return;
			if (IsClosed) {
				m_StartMoveDelta = m_OpeningDelta;
			}
			else if (IsOpen) {
				m_StartMoveDelta = m_ClosingingDelta;
				// vertical movement fading out when a finger is released
				if (m_CurMoveSpeed.Abs() > 0.1f) {
					m_CurMoveSpeed -= m_FadingSpeed * m_CurMoveSpeed;
					return;
				}
			}
			m_CurMoveSpeed = 0;
		}

		public bool IsScrollAllowed {
			get {
//				print($"IS OPEN {IsOpen} IS CLOSED {IsClosed}, {m_HorSlideBuffer}");
				return IsOpen && !m_IsMovingHorizontally;
			}
		}
		
		public bool IsOpen {
			get {
				if (m_NavigationDrawerPosition == NavigationDrawerPosition.Left) {
					return m_SidePanel.anchoredPosition.x >= -1;
				}
				return m_SidePanel.anchoredPosition.x <= 1;
			}
		}

		public bool IsClosed {
			get {
				if (m_NavigationDrawerPosition == NavigationDrawerPosition.Left) {
					return m_SidePanel.anchoredPosition.x <= -(m_DrawerWidth - 1);
				}
				// for the right side
				return m_SidePanel.anchoredPosition.x >= (m_DrawerWidth - 1);
			}
		}

		public NavigationDrawerPosition NavigationDrawerPosition {
			get { return m_NavigationDrawerPosition; }
		}

		private void SetClosed() {
			m_SidePanel.anchoredPosition = new Vector2((float)m_NavigationDrawerPosition * m_DrawerWidth, 0);
			CalculateOpenPercentAndVeilAlpha();
		}
		private void SetOpen() {
			m_SidePanel.anchoredPosition = new Vector2(0, 0);
			CalculateOpenPercentAndVeilAlpha();
		}

		private void CalculateOpenPercentAndVeilAlpha() {
			var pos = m_SidePanel.anchoredPosition;
			m_OpenPercent = (1f - Mathf.Clamp01(pos.x / ((float)m_NavigationDrawerPosition * m_DrawerWidth)));
			if (m_CurrentlyActiveDrawer == this) {
				// only the currently active drawer should set veil alpha
				var alpha = m_OpenPercent * m_MaxAlpha;
				m_VeilCanvasGroup.alpha = alpha;
			}
			OnOpenPercentChange(m_OpenPercent);
		}

		private void OnOpenPercentChange(float value) {
			if (m_ControlButton is HamburgerMenuButton hamburgerMenuButton) {
				hamburgerMenuButton.RotateByValue(value);
			}
		}

		private bool IsMoving {
			get { return m_IsMovingHorizontally || m_IsMovingVertically; }
		}
		

		public void OnDrag(PointerEventData eventData) {
			
			if (m_IsAlwaysOpen) return;
			
			var leftSided = m_NavigationDrawerPosition == NavigationDrawerPosition.Left;
			ChangeGripWidth(false);
			m_IsDragging = true;
			var xMove = eventData.delta.x;
			var yMove = eventData.delta.y;
			m_HorSlideBuffer += xMove.Abs();

			if (!IsMoving) {
				if (yMove.Abs() > xMove.Abs()) {
					m_IsMovingVertically = true;
				}
			}
			
			
			var pos = m_SidePanel.anchoredPosition; 
			if (IsOpen) {
				// не даст ящику начать задвигаться от малейших прикосновений,
				// вместо этого ждет накопления буфера для старта движения
				if (m_HorSlideBuffer < 80) {
					xMove = 0;
				}
			}

			if (m_IsMovingVertically) {
				xMove = 0;
				m_HorSlideBuffer = 0;
			}
			// мешает вертикальной прокрутке, пока не отпустил палец
			// после горизонтального смещения
			if (m_HorSlideBuffer >= 150) {
				m_IsMovingHorizontally = true;
			}
			var min = -m_DrawerWidth;
			var max = 0f;
			if (!leftSided) {
				min = 0;
				max = m_DrawerWidth;
			}
			pos.x = Mathf.Clamp(pos.x + xMove, min, max);
			
			m_SidePanel.anchoredPosition = pos;
			CalculateOpenPercentAndVeilAlpha();

			if (!m_IsMovingHorizontally) {
				var allowMovement = true;
				var pointPosX = eventData.position.x * (m_TotalWidth / Screen.width);
				if (leftSided) {
					var prominence = m_DrawerWidth + pos.x;
					allowMovement = pointPosX <= prominence;
				}
				else {
					// расстояние на которое в данный момент выдвинут ящик
					var prominence = m_DrawerWidth - pos.x;
					allowMovement = pointPosX >= m_TotalWidth - prominence;
				}
				m_CurMoveSpeed = allowMovement ? eventData.delta.y : 0;
			}
			else {
				m_CurMoveSpeed = 0;
			}
		}


		public void OnEndDrag(PointerEventData eventData) {
			m_IsDragging = false;
			m_IsMovingVertically = false;
			m_IsMovingHorizontally = false;
			
			var pos = m_SidePanel.anchoredPosition;
			if (Time.time <= m_DragShouldStopBefore) {
				// meant it's a quick move
				var distX = (m_DragStartPosition.x - eventData.position.x).Abs();
				var distY = (m_DragStartPosition.y - eventData.position.y).Abs();
				if (!IsOpen) {
					distY = 0;
				}
				
				m_TempText.text = ($"DIST X {distX.ToInt()}, DIST Y {(distY * 3).ToInt()}, time {m_DragShouldStopBefore - Time.time}");
				if (distX >= m_MinDistForQuickSwipeOpen && distX > distY * 3) {
					if (m_NavigationDrawerPosition == NavigationDrawerPosition.Left) {
						if (eventData.delta.x >= 0) {
							DoOpen();
						}
						else {
							DoClose();
						}
					}
					else {
						if (eventData.delta.x <= 0) {
							DoOpen();
						}
						else {
							DoClose();
						}
					}
					return;
				}
			}
			
			if (m_NavigationDrawerPosition == NavigationDrawerPosition.Left) {
				var finalPos = -(m_DrawerWidth - (m_DrawerWidth * m_StartMoveDelta));
				if (pos.x >= finalPos) {
					DoOpen();
				}
				else DoClose();
			}
			else {
				// right drawer
				if (pos.x <= m_DrawerWidth * (1 - m_StartMoveDelta)) {
					DoOpen();
				}
				else {
					DoClose();
				}
			}
		}

		private void LerpPanel(float mult = 15f, Action onUpdate = null, Action onComplete = null) {
			if (m_IsAnimating) return;
			StopCoroutine(nameof(LerpPanelCoroutine));
			m_CurrentX = m_SidePanel.anchoredPosition.x;
			StartCoroutine(LerpPanelCoroutine(mult, onUpdate, onComplete));
		}
		
		private IEnumerator LerpPanelCoroutine(float mult = 15f, Action onUpdate = null, Action onComplete = null) {
			m_IsAnimating = true;
			while ((m_CurrentX - m_FinalX).Abs() > .005f) {
				yield return new WaitForSeconds(.015f);
				m_CurrentX = Mathf.Lerp(m_CurrentX, m_FinalX, mult * Time.deltaTime);
				var dist = m_CurrentX - m_FinalX;
				if (dist.Abs() <= 7) {
					if (m_NavigationDrawerPosition == NavigationDrawerPosition.Left) {
						m_OpenPercent = dist >= 0 ? .0f : 1.0f;
					}
					else {
						m_OpenPercent = dist <= 0 ? 1.0f : .0f;
					}
					OnOpenPercentChange(m_OpenPercent);
					m_CurrentX = m_FinalX;
					
				}
				
				if (!m_IsDragging) {
					var panelPosition = m_SidePanel.anchoredPosition;
					panelPosition.x = m_CurrentX;
					m_SidePanel.anchoredPosition = panelPosition;
				}
				else {
					StopCoroutine(nameof(LerpPanelCoroutine));
				}
				onUpdate?.Invoke();
			}

			m_IsAnimating = false;
			onComplete?.Invoke();
		}

		public void DoClose() {
			if (m_IsAnimating) return;
			ChangeGripWidth(false);
			var endPosition = (int) m_NavigationDrawerPosition * m_DrawerWidth;
			m_FinalX = endPosition;
			var timeDelta = Mathf.Max(1 - m_OpenPercent, .55f);
			var animationSpeed = (m_AnimationSpeed * timeDelta);
			LerpPanel(animationSpeed, CalculateOpenPercentAndVeilAlpha, () => {
				m_IsOpenSent = false;
				if (!m_IsCloseSent) {
					m_IsCloseSent = true;
					Close?.Invoke(this);
				}
			});
		}

		/// <summary>
		/// Simply opens the drawer
		/// </summary>
		public void DoOpen(/*bool considerOpenPercent = true*/) {
			if (m_IsAnimating) return;
			StopCoroutine(nameof(LerpPanelCoroutine));
			m_CurrentlyActiveDrawer = this;
			transform.SetAsLastSibling();
			ChangeGripWidth(true);
			m_IsDragging = false;
			m_FinalX = 0;

			CalculateOpenPercentAndVeilAlpha();
			var timeDelta = Mathf.Max(m_OpenPercent, .55f);
			var animationSpeed = (m_AnimationSpeed * timeDelta);
				
			LerpPanel(animationSpeed, CalculateOpenPercentAndVeilAlpha, () => {
				m_IsCloseSent = false;
				if (!m_IsOpenSent) {
					m_IsOpenSent = true;
					Open?.Invoke(this);
				}
			});

			
		}


		
	}
}