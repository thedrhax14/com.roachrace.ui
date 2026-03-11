using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using RoachRace.UI.Core;

namespace RoachRace.UI.Components
{
    /// <summary>
    /// Base class for UI windows that can be shown or hidden.<br>
    /// Typical usage: assign a UI <see cref="Toggle"/> (the "open" toggle) and a <see cref="CanvasGroup"/> for visibility and interaction control.<br>
    /// Windows auto-register with <see cref="UIWindowManager"/> for type-safe retrieval, and the manager wires toggle changes to show/hide behavior.
    /// </summary>
    public abstract class UIWindow : MonoBehaviour
    {
        [SerializeField] private Toggle openToggle;

        [SerializeField] private bool autoRenameOpenToggleObject = true;

        /// <summary>
        /// Optional toggle that controls whether this window is visible.<br>
        /// When <c>true</c>, the window is shown; when <c>false</c>, the window is hidden.<br>
        /// This is typically wired by <see cref="UIWindowManager"/> during registration.
        /// </summary>
        public Toggle OpenToggle => openToggle;

        /// <summary>
        /// The action registered with <see cref="Toggle.onValueChanged"/> for <see cref="OpenToggle"/>.<br>
        /// Managed by <see cref="UIWindowManager"/> to ensure listeners are removed on unregister.
        /// </summary>
        public UnityAction<bool> OpenToggleAction { get; set; }

        [SerializeField] protected CanvasGroup _canvasGroup;

        /// <summary>
        /// Unity editor callback used to keep the hierarchy clean when references are assigned in the Inspector.<br>
        /// When enabled, this auto-renames the assigned <see cref="OpenToggle"/>'s GameObject to a predictable name.<br>
        /// This runs in edit mode (not at runtime) and is safe to disable via <c>autoRenameOpenToggleObject</c>.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (!autoRenameOpenToggleObject)
                return;

            if (Application.isPlaying)
                return;

            if (openToggle == null)
                return;

            string desiredName = $"{GetType().Name}_OpenToggle";
            if (openToggle.gameObject.name != desiredName)
            {
                openToggle.gameObject.name = desiredName;
            }
        }

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                Debug.LogWarning($"[{nameof(UIWindow)}] CanvasGroup missing on '{gameObject.name}'. Falling back to SetActive-based visibility.", gameObject);
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
            SetVisible(true);
            OnShow();
        }

        public virtual void Hide()
        {
            SetVisible(false);
            OnHide();
        }

        /// <summary>
        /// Sets the visibility of the window.<br>
        /// Uses <see cref="CanvasGroup"/> alpha and raycast blocking when available, falling back to <see cref="GameObject.SetActive"/> otherwise.
        /// </summary>
        /// <param name="visible">If <c>true</c> show the window; otherwise hide it.</param>
        public void SetVisible(bool visible)
        {
            Debug.Log($"[{nameof(UIWindow)}] Setting visibility of '{gameObject.name}' to {visible}", gameObject);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
            gameObject.SetActive(visible);
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
                Debug.LogWarning($"[{nameof(UIWindow)}] Cannot set interactable state on '{gameObject.name}' because CanvasGroup is missing.", gameObject);
            }
        }

        /// <summary>
        /// Gets whether the window is currently visible.<br>
        /// When a <see cref="CanvasGroup"/> exists, visibility is derived from its alpha/raycast state; otherwise, from <see cref="GameObject.activeSelf"/>.
        /// </summary>
        public bool IsVisible => _canvasGroup != null
            ? _canvasGroup.alpha > 0.001f && _canvasGroup.blocksRaycasts
            : gameObject.activeSelf;

        protected virtual void OnShow() { }
        
        protected virtual void OnHide() { }
    }
}
