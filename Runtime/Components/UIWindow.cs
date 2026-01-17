using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using RoachRace.UI.Core;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Base class for UI windows that can be shown or hidden
    /// Auto-registers with UIWindowManager for type-safe retrieval
    /// Must be inherited - cannot be used directly
    /// </summary>
    public abstract class UIWindow : MonoBehaviour
    {
        [SerializeField] private Button openButton;
        public Button OpenButton => openButton;
        public UnityAction OpenAction { get; set; }

        [SerializeField] protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                //Debug.LogWarning($"[UIWindow] CanvasGroup missing on '{gameObject.name}'. Window interaction control will not work.", gameObject);
            }

            // Auto-register with UIWindowManager
            UIWindowManager.Instance?.RegisterWindow(this);
        }

        protected virtual void Start() { }

        protected virtual void OnDestroy()
        {
            // Auto-unregister from UIWindowManager
            if (UIWindowManager.HasInstance)
                UIWindowManager.Instance?.UnregisterWindow(this);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if(_canvasGroup) _canvasGroup.alpha = 1f;
            OnShow();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            if(_canvasGroup) _canvasGroup.alpha = 0f;
            OnHide();
        }

        public void SetInteractable(bool interactable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = interactable;
                _canvasGroup.blocksRaycasts = interactable;
            }
            else
            {
                Debug.LogWarning($"[UIWindow] Cannot set interactable state on '{gameObject.name}' because CanvasGroup is missing.", gameObject);
            }
        }

        public bool IsVisible => gameObject.activeSelf;

        protected virtual void OnShow() { }
        
        protected virtual void OnHide() { }
    }
}
