using System;
using System.Collections;
using Components.Drawer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Components.Pages {
	
	[DisallowMultipleComponent][ExecuteInEditMode]
	public class PageBase : InteractiveObject, IDragHandler, IBeginDragHandler, IEndDragHandler {

	    
        private Action m_OnAnimactionComplete;
        protected bool p_IsAnimating = false;
		
        public bool RequireConfirmationOnBack { get; internal set; }
		// any page can be added to a navigation drawer's container, so it might be necessary 
		// to disable some page's features (e.g. vertical scrolling) while the drawer is not open.
		// Just check this property for null to make some decision you need
		// for usage example see ParallaxScrollPage class's source
		protected NavigationDrawer NavigationDrawer { get; private set; }

        protected virtual void Awake() {
	        ParentChanged += OnParentChanged;
        }
		private void OnParentChanged(InteractiveObject target, Transform newParent) {
			NavigationDrawer = newParent == null 
				? null : newParent.gameObject.GetComponentInParent<NavigationDrawer>();
		}
		/// <summary>
		/// Called after the page is added and all accompanying animations are complete
		/// </summary>
        protected virtual void OnAfterAppeared() {
            
        }
        
        protected virtual void OnAdded() {
            
        }
        
        protected virtual void OnRemoved() {
            
        }

        public bool Enabled {
            get { return gameObject.activeInHierarchy; }
            set {
                gameObject.SetActive(value);
                if (value) {
                    transform.SetAsLastSibling();
                    if (GetRectTransform() != null) {
                        OnAdded();
                    }
                }
                else {
                    OnRemoved();
                }
            }
        }

        public void AnimateAppear(Action onComplete = null) {
            if (p_IsAnimating) return;
            Enabled = true;
            p_IsAnimating = true;
            m_OnAnimactionComplete = onComplete;
	        StartCoroutine(AppearCoroutine());
        }

	    private IEnumerator AppearCoroutine(float startScale = .95f, float endScale = 1.0f, float animationTime = .25f, float delaySeconds = 0.0f) {
	        yield return new WaitForSeconds(delaySeconds);
		    var rectTransform = GetRectTransform();
		    var curScale = rectTransform.localScale.x;
		    if (curScale > endScale) {
			    curScale = endScale;
		    }
		    rectTransform.localScale = new Vector3(curScale, curScale, 1);
		    var iterations = animationTime / 0.016f;
		    var scaleStep = (endScale - startScale) / iterations;
		    
	        while (curScale < endScale) {
	            yield return new WaitForSeconds(.016f);
		        curScale += scaleStep;
		        curScale = Mathf.Clamp01(curScale);
		        rectTransform.localScale = new Vector3(curScale, curScale, 1);
	        }
	        
	    } 

        public void ShowWithoutAnimation() {
            Enabled = true;
            GetRectTransform().anchoredPosition = new Vector2();
            OnAfterAppeared();
        }
        
        private void OnAnimationComplete() {
            p_IsAnimating = false;
            m_OnAnimactionComplete?.Invoke();
            m_OnAnimactionComplete = null;
            OnAfterAppeared();
           
        }

		public virtual void OnDrag(PointerEventData eventData) {
			ForwardToParents<IDragHandler>((parent) => parent.OnDrag(eventData));
		}
		public virtual void OnBeginDrag(PointerEventData eventData) {
			ForwardToParents<IBeginDragHandler>((parent) => parent.OnBeginDrag(eventData));
		}

		public virtual void OnEndDrag(PointerEventData eventData) {
			ForwardToParents<IEndDragHandler>((parent) => parent.OnEndDrag(eventData));
		}
	}
}